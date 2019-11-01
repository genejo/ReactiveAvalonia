using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl;
using Flurl.Http;
using System.Threading;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;

namespace ReactiveAvalonia.RandomBuddyStalker {

    // This guy shows quite a bit
    // [Archive](https://www.nequalsonelifestyle.com/archive/#2019)

    public class MainViewModel : ReactiveObject, IActivatableViewModel {
        public ViewModelActivator Activator { get; }

        private const int DecisionTime = 1000;
        private const int hourMsCount = 3600000;

        private int GetPseudoTimeNowMs() {
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % hourMsCount);
        }
        private int GetTimeSinceStart() {
            return GetPseudoTimeNowMs() - _startTime;
        }
        private int GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }

        private int _startTime;

        private volatile int _lastDecisionStartTime;

        private int _decisionsLeftCount = 10;

        Random randy = new Random();

        // better solution for pausing:
        // https://leeoades.wordpress.com/2012/12/17/pausable-observable/
        //private static volatile bool _isPaused = false;

        public MainViewModel() {
            IsTimerRunning = false;

            Activator = new ViewModelActivator();
            this.WhenActivated(
                disposables => {
                    //var timeKeeper = RunTimeKeeperAsync();
                    Console.WriteLine($"[vm {GetThreadId()}]: ViewModel activated");

                    Disposable
                        .Create(
                            () => {
                                //_frenzyOn = false;
                                // TODO: see if I can use a timeout?
                                //timeKeeper.Wait();
                                Console.WriteLine(
                                    $"[vm {GetThreadId()}]: " +
                                    "ViewModel deactivated");
                            })
                        .DisposeWith(disposables);

                    // Observable
                    //     .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(3000))
                    //     .ObserveOn(RxApp.MainThreadScheduler)
                    //     .Subscribe(_ => {
                    //         Remaining = 100 - Remaining;
                    //     })
                    //     .DisposeWith(disposables);

                    // Observable
                    //     .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500))
                    //     .Where(_ => _isPaused == false)
                    //     .Take(5)
                    //     .Select(_ => GetRandomUser())
                    //     .ObserveOn(RxApp.MainThreadScheduler)
                    //     .Subscribe(
                    //         x => {
                    //             Console.WriteLine(
                    //                 $"[sb {GetThreadId()}]: " +
                    //                 $"|{GetTimeSinceStart()}| " +
                    //                 $"=> {x}");
                    //         },
                    //         err => Console.WriteLine($"error: {err}"),
                    //         () => Console.WriteLine("Stalking quota reached... Try later please :) !"))
                    //     .DisposeWith(disposables);


                    // You, I and ReactiveUI: 14.6 - scheduling
                    
                });

            var canInitiateNewFetch =
                this.WhenAnyValue(vm => vm.IsFetching, fetching => !fetching);

            // https://reactiveui.net/docs/handbook/scheduling/
            // https://blog.jonstodle.com/task-toobservable-observable-fromasync-task/
            // https://github.com/reactiveui/ReactiveUI/issues/1245
            StalkCommand =
                ReactiveCommand.CreateFromObservable(
                    () => Observable.StartAsync(Stalk),
                    canInitiateNewFetch,
                    RxApp.MainThreadScheduler
                );

            ContinueCommand =
                ReactiveCommand.CreateFromObservable(
                    () => Observable.StartAsync(Continue),
                    canInitiateNewFetch,
                    RxApp.MainThreadScheduler
                );

            //https://reactiveui.net/docs/handbook/when-activated/#no-need
            ContinueCommand.Execute().Subscribe();

            // https://reactiveui.net/docs/handbook/commands/canceling#canceling-via-another-observable
            var cancel = new Subject<Unit>();
            var cmd = ReactiveCommand.CreateFromObservable(
                 () => Observable
                .Return(Unit.Default)
                .Delay(TimeSpan.FromSeconds(3))
                .TakeUntil(cancel)
                );

            cmd.Subscribe(_ => System.Console.WriteLine("!"));

            this
                .WhenAnyObservable(vm => vm.TriggeringTimer)
                .Do(trigger => {
                    IsTimerRunning = trigger == TimerTrigger.Start;
                    Remaining = IsTimerRunning ? 80 : 0;
                    System.Console.WriteLine($"[tt {GetThreadId()}] running={IsTimerRunning}");
                    cancel.OnNext(Unit.Default);
                    cmd.Execute().Subscribe();
                })
                .Subscribe();
        }


        [Reactive]
        public double Remaining { get; private set; }

        [Reactive]
        public string BuddyName { get; private set; }

        [Reactive]
        public bool IsFetching { get; private set; }

        [Reactive]
        public bool IsTimerRunning { get; private set; }

        public ReactiveCommand<Unit, Unit> StalkCommand { get; }
        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private string _userAvatarUrl;

        private async Task Stalk() {
            _triggeringTimer.OnNext(TimerTrigger.Stop);
            
            IsFetching = true;
            // TODO: check if url is valid
            System.Console.WriteLine($"[{GetThreadId()}]...fetching avatar");
            byte[] bytes = await _userAvatarUrl.GetBytesAsync();

            // TODO: set bytes to reactive Image
            System.Console.WriteLine($"[{GetThreadId()}]...fetched {bytes.Length} bytes");
            IsFetching = false;
        }

        public enum TimerTrigger { Start, Stop };

        //https://rehansaeed.com/reactive-extensions-part1-replacing-events/
        private readonly Subject<TimerTrigger> _triggeringTimer = new Subject<TimerTrigger>();
        public IObservable<TimerTrigger> TriggeringTimer => _triggeringTimer.AsObservable();

        private readonly Random _randomizer = new Random();
        private async Task Continue() {
            IsFetching = true;
            int userId = _randomizer.Next() % 12 + 1;
            System.Console.WriteLine($"[{GetThreadId()}]...fetching data for random user {userId}");
            var userDtoFetcherTask =
                "https://reqres.in/api/"
                    .AppendPathSegments("users", userId)
                    .GetJsonAsync<UserDto>();

            // https://stackoverflow.com/questions/14455293/how-and-when-to-use-async-and-await
            // https://medium.com/rubrikkgroup/understanding-async-avoiding-deadlocks-e41f8f2c6f5d
            var user = await userDtoFetcherTask;

            _userAvatarUrl = user.Data.AvatarUrl;
            BuddyName = $"{user.Data.FirstName} {user.Data.LastName}";
            System.Console.WriteLine($"[{GetThreadId()}] user avatar url: {_userAvatarUrl}");
            IsFetching = false;

            _triggeringTimer.OnNext(TimerTrigger.Start);
        }
    }

    // https://reqres.in/api/users/1
    // https://stackoverflow.com/questions/725348/plain-old-clr-object-vs-data-transfer-object
    public class UserDto {
        public class DataDto {
            [JsonProperty("id")]
            public int Id {get; set; }
            [JsonProperty("email")]
            public string Email {get; set;}
            [JsonProperty("first_name")]
            public string FirstName {get; set;}
            [JsonProperty("last_name")]
            public string LastName {get; set;}
            [JsonProperty("avatar")]
            public string AvatarUrl {get; set; }
        }

        [JsonProperty("data")]
        public UserDto.DataDto Data {get; set;}
    }

}

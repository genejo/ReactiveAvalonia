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
            IsTimerPaused = true;

            Activator = new ViewModelActivator();
            this.WhenActivated(
                disposables => {
                    Console.WriteLine($"[vm {GetThreadId()}]: ViewModel activated" + '\n');

                    Disposable
                        .Create(
                            () => {
                                Console.WriteLine(
                                    $"[vm {GetThreadId()}]: ViewModel deactivated" + '\n');
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

            this
                .WhenAnyValue(vm => vm.IsTimerPaused)
                .Do(
                    paused => {
                        Remaining = paused ? 0 : 80;
                        System.Console.WriteLine($"IsTimerPaused = {paused}");
                    })
                .Subscribe();

            var canInitiateNewFetch =
                this.WhenAnyValue(vm => vm.IsFetching, fetching => !fetching);

            // https://reactiveui.net/docs/handbook/scheduling/
            // https://blog.jonstodle.com/task-toobservable-observable-fromasync-task/
            // https://github.com/reactiveui/ReactiveUI/issues/1245
            StalkOrContinueCommand =
                ReactiveCommand.CreateFromObservable(
                    () => Observable.StartAsync(StalkOrContinue),
                    canInitiateNewFetch,
                    RxApp.MainThreadScheduler
                );
        }

        private readonly Random _randomizer = new Random();

        [Reactive]
        public double Remaining { get; private set; }

        [Reactive]
        public string BuddyName { get; private set; }

        [Reactive]
        public bool IsFetching { get; private set; }

        [Reactive]
        public bool IsTimerPaused { get; private set; }

        

        public ReactiveCommand<Unit, Unit> StalkOrContinueCommand { get; }
        private async Task StalkOrContinue() {
            IsFetching = true;
            System.Console.WriteLine($"[{GetThreadId()}]...doing");

            if (IsTimerPaused) {
                await Continue();
            }
            else {
                await Stalk();
            }

            IsTimerPaused = !IsTimerPaused;
            IsFetching = false;
            System.Console.WriteLine($"[{GetThreadId()}]...done\n");
        }

        private string _userAvatarUrl;

        private async Task Stalk() {
            // TODO: check if url is valid
            System.Console.WriteLine($"[{GetThreadId()}]...fetching avatar");
            byte[] bytes = await _userAvatarUrl.GetBytesAsync();
            System.Console.WriteLine($"[{GetThreadId()}]...fetched {bytes.Length} bytes");
        }
        private async Task Continue() {
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
            System.Console.WriteLine($"[{GetThreadId()}] user avatar url: {_userAvatarUrl}");
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

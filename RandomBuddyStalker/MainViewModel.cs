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

        private int GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }

        public readonly static int DecisionTimeSeconds = 2;


        public MainViewModel() {
            IsTimerRunning = false;

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

            // Run the "Continue" command once in the beginning in order to
            // fetch the first buddy.
            // https://reactiveui.net/docs/handbook/when-activated/#no-need
            ContinueCommand.Execute().Subscribe();

            // https://reactiveui.net/docs/handbook/commands/canceling#canceling-via-another-observable
            var startTimerCommand = ReactiveCommand.CreateFromObservable(
                    () =>
                        Observable
                            .Return(Unit.Default)
                            .Delay(TimeSpan.FromSeconds(DecisionTimeSeconds))
                            .TakeUntil(
                                _triggeringTheTimer
                                    .Where(trigger => trigger == TimerTrigger.Stop)));

            startTimerCommand.Subscribe(_ => 
                ContinueCommand.Execute().Subscribe());

            this
                .WhenAnyObservable(vm => vm.TriggeringTheTimer)
                .Do(trigger => {
                    if (trigger == TimerTrigger.Start) {
                        startTimerCommand.Execute().Subscribe();
                        IsTimerRunning = true;
                    }
                    else {
                        IsTimerRunning = false;
                    }                    
                })
                .Subscribe();
        }

        [Reactive]
        public string BuddyName { get; private set; }

        [Reactive]
        public bool IsTimerRunning { get; private set; }

        [Reactive]
        private bool IsFetching { get; set; }

        public ReactiveCommand<Unit, Unit> StalkCommand { get; }
        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private string _userAvatarUrl;

        private async Task Stalk() {
            _triggeringTheTimer.OnNext(TimerTrigger.Stop);
            
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
        private readonly Subject<TimerTrigger> _triggeringTheTimer = new Subject<TimerTrigger>();
        private IObservable<TimerTrigger> TriggeringTheTimer => _triggeringTheTimer.AsObservable();

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

            _triggeringTheTimer.OnNext(TimerTrigger.Start);
        }
    }

    // User .json example: https://reqres.in/api/users/1
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

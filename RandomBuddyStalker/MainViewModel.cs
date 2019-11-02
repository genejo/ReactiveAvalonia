using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;
using System.IO;
using Avalonia.Media.Imaging;

namespace ReactiveAvalonia.RandomBuddyStalker {

    // This guy shows quite a bit
    // [Archive](https://www.nequalsonelifestyle.com/archive/#2019)

    public class MainViewModel : ReactiveObject {
        public readonly static int DecisionTimeMilliseconds = 2000;

        public MainViewModel() {
            IsTimerRunning = false;

            var canInitiateNewFetch =
                this.WhenAnyValue(vm => vm.Fetching, fetching => !fetching);

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

            // Run the "Continue" command once in the beginning in order to fetch the first buddy.
            // https://reactiveui.net/docs/handbook/when-activated/#no-need
            ContinueCommand.Execute().Subscribe();

            // https://reactiveui.net/docs/handbook/commands/canceling#canceling-via-another-observable
            var startTimerCommand = ReactiveCommand.CreateFromObservable(
                    () =>
                        Observable
                            .Return(Unit.Default)
                            .Delay(TimeSpan.FromMilliseconds(DecisionTimeMilliseconds))
                            .TakeUntil(
                                TriggeringTheTimer
                                    .Where(trigger => trigger == TimerTrigger.Stop)));

            startTimerCommand.Subscribe(_ => ContinueCommand.Execute().Subscribe());

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

        // TODO: reference ReactiveUI.Fody.Helpers
        [Reactive]
        public string BuddyName { get; private set; }

        [Reactive]
        public Bitmap BuddyAvatar { get; private set; }

        [Reactive]
        public bool IsTimerRunning { get; private set; }

        [Reactive]
        private bool Fetching { get; set; }

        public ReactiveCommand<Unit, Unit> StalkCommand { get; }
        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }

        private string _userAvatarUrl;

        public enum TimerTrigger { Start, Stop };

        //https://rehansaeed.com/reactive-extensions-part1-replacing-events/
        private readonly Subject<TimerTrigger> _triggeringTheTimer = new Subject<TimerTrigger>();
        public IObservable<TimerTrigger> TriggeringTheTimer => _triggeringTheTimer.AsObservable();

        private async Task Stalk() {
            _triggeringTheTimer.OnNext(TimerTrigger.Stop);
            
            Fetching = true;
            // TODO: check if url is valid
            byte[] bytes = await _userAvatarUrl.GetBytesAsync();

            // TODO: check if this is needed
            BuddyAvatar?.Dispose();
            BuddyAvatar = new Bitmap(new MemoryStream(bytes));

            // TODO: set bytes to reactive Image
            Fetching = false;
        }

        private readonly Random _randomizer = new Random();
        private async Task Continue() {
            Fetching = true;
            BuddyAvatar = null;
            int userId = _randomizer.Next() % 12 + 1;
            var userDtoFetcherTask =
                    "https://reqres.in/api/"
                        .AppendPathSegments("users", userId)
                        .GetJsonAsync<UserDto>();

            // https://stackoverflow.com/questions/14455293/how-and-when-to-use-async-and-await
            // https://medium.com/rubrikkgroup/understanding-async-avoiding-deadlocks-e41f8f2c6f5d
            var user = await userDtoFetcherTask;

            _userAvatarUrl = user.Data.AvatarUrl;
            BuddyName = $"{user.Data.FirstName} {user.Data.LastName}";
            Fetching = false;

            _triggeringTheTimer.OnNext(TimerTrigger.Start);
        }
    }
}

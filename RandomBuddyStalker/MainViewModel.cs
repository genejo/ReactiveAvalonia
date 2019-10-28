using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl.Http;
using System.Threading;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
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

        private int _decisionsLeftCount = 4;

        Random randy = new Random();

        // better solution for pausing:
        // https://leeoades.wordpress.com/2012/12/17/pausable-observable/
        //private static volatile bool _isPaused = false;

        public MainViewModel() {
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
                    this
                        .WhenAnyValue(vm => vm.Delta)
                        //.Sample(TimeSpan.FromMilliseconds(30))
                        //.Select(delta => DecisionTime - delta)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        //.BindTo(this, vm => vm.Remaining);
                        .Subscribe(delta => {
                            //Remaining = DecisionTime - delta;
                        })
                        .DisposeWith(disposables);
                        
                        //.ObserveOn(RxApp.MainThreadScheduler)
                        // .Do(delta => {
                        //     //Remaining = Math.Max(0, DecisionTime - delta).ToString();
                        //     var rem = Math.Max(0, DecisionTime - delta).ToString();
                        //     System.Console.WriteLine($"rem: {rem}");
                        //     var xx = randy.Next() % 4 + delta;
                        //     Remaining = xx;
                        //     //Remaining = rem;
                        // })
                        // .Subscribe();
                    
                });

            var canExecute =
                this.WhenAnyValue(vm => vm.IsFetching, x => !x);

            // https://reactiveui.net/docs/handbook/scheduling/
            //PerformCommand = ReactiveCommand.Create(Perform, canExecute, RxApp.MainThreadScheduler);


            PerformCommand = ReactiveCommand.CreateFromObservable(
                () =>
                    Observable
                        .Return(Unit.Default)
                        .ObserveOn(RxApp.TaskpoolScheduler)
                        .Do(_ => {
                            RxApp.MainThreadScheduler.Schedule(() => {
                                IsTimerPaused = !IsTimerPaused;
                                Remaining = 75 - Remaining;
                                IsFetching = true;
                                System.Console.WriteLine($"[do-{GetThreadId()}]");
                            });
                            System.Console.WriteLine($"[do_{GetThreadId()}]");
                            Thread.Sleep(1000);
                            RxApp.MainThreadScheduler.Schedule(() => {
                                IsFetching = false;
                                System.Console.WriteLine($"[do+{GetThreadId()}]");
                            });
                        }),
                canExecute);

            //Remaining = 123;
        }

        Random _randomizer = new Random();

        [Reactive]
        public double Remaining { get; set; }

        [Reactive]
        public string BuddyName { get; set; }

        [Reactive]
        public int Delta { get; set; }

        [Reactive]
        public bool IsFetching { get; set; }

        [Reactive]
        public bool IsTimerPaused { get; set; }

        public ReactiveCommand<Unit, Unit> PerformCommand { get; }
        private void Perform() {
            RxApp.MainThreadScheduler.Schedule(() => {
                IsTimerPaused = !IsTimerPaused;
                Remaining = 75 - Remaining;
                IsFetching = true;
                System.Console.WriteLine($"[sc-{GetThreadId()}]: ");
            });
            System.Console.WriteLine($"[pr {GetThreadId()}]: " );
            Thread.Sleep(1000);

            RxApp.MainThreadScheduler.Schedule(() => {
                IsFetching = false;
                System.Console.WriteLine($"[sc+{GetThreadId()}]: ");
            });
        }
        // private string GetRandomUser() {
        //     System.Console.WriteLine(
        //         $"[gs>{GetThreadId()}]: " +
        //         $"|{GetTimeSinceStart()}| => Asking for string");
        //     int userId = _randomizer.Next() % 12 + 1;            
        //     var tasky = $"https://reqres.in/api/users/{userId}".GetStringAsync();
        //     tasky.Wait();
        //     var getResp = tasky.Result.Substring(13, 2);
        //     System.Console.WriteLine(
        //         $"[gs<{GetThreadId()}]: " +
        //         $"|{GetTimeSinceStart()}| => String received = {getResp}");
        //     return getResp;
        // }
    }
}

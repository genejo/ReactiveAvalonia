using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl.Http;
using System.Threading;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainViewModel : ReactiveObject, IActivatableViewModel {
        public ViewModelActivator Activator { get; }

        private static int DecisionTime = 1000;

        private int GetPseudoTimeNowMs() {
            // 10000000 ms - a bit less than 3 hours
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000000);
        }
        private int GetTimeSinceStart() {
            return GetPseudoTimeNowMs() - _startTime;
        }
        private int GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }

        private int _startTime;

        private volatile int _lastDecisionStartTime;

        //private object _locker = new object();

        private int _decisionsLeftCount = 3;

        Random randy = new Random();

        // better solution for pausing:
        // https://leeoades.wordpress.com/2012/12/17/pausable-observable/
        //private static volatile bool _isPaused = false;

        public MainViewModel() {
            Activator = new ViewModelActivator();

            this.WhenActivated(
                disposables => {
                    var timeKeeper = RunTimeKeeperAsync();
                    Console.WriteLine($"[vm {GetThreadId()}]: ViewModel activated");

                    Disposable
                        .Create(
                            () => {
                                _frenzyOn = false;
                                // TODO: see if I can use a timeout?
                                timeKeeper.Wait();
                                Console.WriteLine(
                                    $"[vm {GetThreadId()}]: " +
                                    "ViewModel deactivated");
                            })
                        .DisposeWith(disposables);
                    
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
                    // this
                    //     .WhenAnyValue(vm => vm.Delta)
                    //     .Sample(TimeSpan.FromMilliseconds(160))
                    //     .Select(delta => DecisionTime - delta)
                    //     .ObserveOn(RxApp.MainThreadScheduler)
                    //     //.BindTo(this, vm => vm.Remaining);
                    //     .Subscribe(x => {
                    //         Remaining = x;
                    //         System.Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}!{x}");
                    //     })
                    //     .DisposeWith(disposables);
                        
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
                    
                    // this
                    //     .WhenAnyValue(vm => vm.Delta)
                    //     .Do(
                    //         x => {
                    //             lock (_locker) {
                    //                     System.Console.WriteLine($"Left: {_decisionsLeftCount}");
                    //                     if (_decisionsLeftCount == 0)
                    //                         _frenzyOn = false;
                    //                 }
                    //             }

                    //             System.Console.WriteLine($"deltuta: {x,5}");
                    //         }
                    //     )
                    //     .Subscribe();
                });
            Remaining = 123;
        }

        Random _randomizer = new Random();

        [Reactive]
        public int Remaining { get; set; }

        [Reactive]
        public string BuddyName { get; set; }

        [Reactive]
        public int Delta { get; set; }

        private bool _frenzyOn = true;
        private async Task RunTimeKeeperAsync() {
            await Observable.Start(() => {
                _startTime = GetPseudoTimeNowMs();
                _lastDecisionStartTime = GetPseudoTimeNowMs();
                while (_frenzyOn) {
                    //lock (_locker) {
                        int newDelta = GetPseudoTimeNowMs() - _lastDecisionStartTime;

                        if (newDelta > DecisionTime) {
                            _decisionsLeftCount--;
                            System.Console.WriteLine("@@@@" + _decisionsLeftCount);
                            if (_decisionsLeftCount == 0)
                                break;

                            _lastDecisionStartTime += DecisionTime;
                            newDelta -= DecisionTime;
                        }

                        Delta = newDelta;
                    //}
                    Thread.Sleep(30);
                }
            }, RxApp.TaskpoolScheduler);
        }

        private string GetRandomUser() {
            System.Console.WriteLine(
                $"[gs>{GetThreadId()}]: " +
                $"|{GetTimeSinceStart()}| => Asking for string");
            int userId = _randomizer.Next() % 12 + 1;            
            var tasky = $"https://reqres.in/api/users/{userId}".GetStringAsync();
            tasky.Wait();
            var getResp = tasky.Result.Substring(13, 2);
            System.Console.WriteLine(
                $"[gs<{GetThreadId()}]: " +
                $"|{GetTimeSinceStart()}| => String received = {getResp}");
            return getResp;
        }
    }
}

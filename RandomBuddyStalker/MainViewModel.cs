using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl.Http;
using System.Threading;
using System.Reactive.Disposables;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainViewModel : ReactiveObject, IActivatableViewModel {
        public ViewModelActivator Activator { get; }
        private readonly long _startTime;
        private int GetTimeSinceStart() {
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _startTime);
        }
        private int GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }

        // better solution for pausing:
        // https://leeoades.wordpress.com/2012/12/17/pausable-observable/
        private static volatile bool _isPaused = false;
        public MainViewModel() {
            Activator = new ViewModelActivator();

            _startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.WhenActivated(
                disposables => {
                    Console.WriteLine(
                        $"[vm {GetThreadId()}]: " +
                        "ViewModel activated");

                    Disposable
                        .Create(
                            () => 
                                Console.WriteLine(
                                    $"[vm {GetThreadId()}]: " +
                                    "ViewModel deactivated"))
                        .DisposeWith(disposables);

                    // Observable
                    //     .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500))
                    //     .Take(5)
                    //     //.Select(_ => Observable.FromAsync(async () => await GetRandomUser()))
                    //     .Select(_ => Observable.FromAsync(async () => await GetRandomUserAsync()))
                    //     // https://github.com/dotnet/reactive/issues/459
                    //     // https://blog.jonstodle.com/flattening-observables-in-rx-net/
                    //     .Concat()
                    //     //.Merge()
                    //     .ObserveOn(RxApp.MainThreadScheduler)
                    //     .Subscribe(
                    //         x => {
                    //             Console.WriteLine(
                    //                 $"[sb {GetThreadId()}]: " +
                    //                 $"|{GetTimeSinceStart()}| " +
                    //                 $"=> {x}");
                    //         },
                    //         err => Console.WriteLine($"error: {err}"),
                    //         () => Console.WriteLine("Quota reached... Try later please :) !"))
                    //     .DisposeWith(disposables);
                    
                    Observable
                        .Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500))
                        .Where(_ => _isPaused == false)
                        .Take(5)
                        .Select(_ => GetRandomUser())
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(
                            x => {
                                Console.WriteLine(
                                    $"[sb {GetThreadId()}]: " +
                                    $"|{GetTimeSinceStart()}| " +
                                    $"=> {x}");
                            },
                            err => Console.WriteLine($"error: {err}"),
                            () => Console.WriteLine("Stalking quota reached... Try later please :) !"))
                        .DisposeWith(disposables);
                });
        }

        Random _randomizer = new Random();

        [Reactive]
        public string BuddyName { get; set; }

        // https://blog.jonstodle.com/task-toobservable-observable-fromasync-task/
        // private async Task<string> GetRandomUserAsync() {
        //     System.Console.WriteLine(
        //         $"[ga>{GetThreadId()}]: " +
        //         $"|{GetTimeSinceStart()}| => Asking for string");
        //     // Last time I checked there were 12 users with id's from [1..12]
        //     int userId = _randomizer.Next() % 12 + 1;
        //     string getResp = await $"https://reqres.in/api/users/{userId}".GetStringAsync();
        //     getResp = getResp.Substring(13, 2);
        //     //Thread.Sleep(2000);
        //     System.Console.WriteLine(
        //         $"[ga<{GetThreadId()}]: " +
        //         $"|{GetTimeSinceStart()}| => String received = {getResp}");
        //     return getResp;
        // }

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

using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Flurl.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Disposables;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainViewModel : ReactiveObject, IActivatableViewModel {
        public ViewModelActivator Activator { get; }
        public MainViewModel() {
            Activator = new ViewModelActivator();

            this.WhenActivated(
                disposables => {
                    Console.WriteLine($"[vm {Thread.CurrentThread.ManagedThreadId}]: ViewModel activated");

                    Disposable
                        .Create(() => Console.WriteLine($"[vm {Thread.CurrentThread.ManagedThreadId}]: ViewModel deactivated"))
                        .DisposeWith(disposables);

                    Observable
                        .Interval(TimeSpan.FromSeconds(3))
                        .Take(10)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Select(_ => Observable.FromAsync(async () => await GetRandomUser()))
                        .Concat()
                        .Subscribe(
                            x => Console.WriteLine($"[vm {Thread.CurrentThread.ManagedThreadId}]: ++ {x}"),
                            err => Console.WriteLine($"error: {err}"),
                            () => Console.WriteLine("Quota reached... Try later please :) !"))
                        .DisposeWith(disposables);
                    //Observable
                    //    .Interval(TimeSpan.FromSeconds(1))
                    //    .ObserveOn(RxApp.MainThreadScheduler)
                    //    .Select(_ => Observable.FromAsync(async () => await GetRandomUser()))
                    //    .Concat()
                    //    .Subscribe(
                    //        user => {
                    //            Console.WriteLine($"[vm {Thread.CurrentThread.ManagedThreadId}]: {user}\n");
                    //        })
                    //    .DisposeWith(disposables);

                });
                //IObservable<string> stringobs;
                //stringobs
                //    .SelectMany(GetRandomUser)
                //    .Subscribe;

                //Observable
                //    .Interval(TimeSpan.FromSeconds(1))
                //    .ObserveOn(RxApp.MainThreadScheduler)
                //    .Select(_ => Observable.FromAsync(async () => await GetRandomUser()))
                //    .Concat()
                //    .Subscribe(user => Console.WriteLine($"{user}\n"));

                //Observable
                //    .FromAsync(async () => await GetRandomUser())
                //    .Subscribe(user => Console.WriteLine($"{user}"));

                //Observable
                //.Interval(TimeSpan.FromSeconds(2))
                //.Select(_ => Observable.FromAsync(async () => await GetRandomUser()))
                //.Subscribe(user => Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}"));

        }

        Random _randomizer = new Random();

        [Reactive]
        public string LeftBuddyName { get; set; }

        [Reactive]
        public string RightBuddyName { get; set; }

        private async Task<string> GetRandomUser() {
            // Last time I checked there were 1 users with id's in [1..12]
            int userId = _randomizer.Next() % 12 + 1;
            string getResp = await $"https://reqres.in/api/users/{userId}".GetStringAsync();
            //Console.Write($"[g  {Thread.CurrentThread.ManagedThreadId}]... ");
            return getResp;
        }
    }
}

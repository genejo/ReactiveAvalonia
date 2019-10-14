using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace ReactiveAvalonia.HelloWorld {
    public class MainViewModel : ReactiveObject {
        public MainViewModel() {
            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(t => Greeting = $"Hello, {Adjectives[t % Adjectives.Length]} world !")
                .Subscribe();

            this
                .WhenAnyValue(vm => vm.Greeting)
                .Do(name => Console.WriteLine($"[vm]: Greeting just became: \"{name}\""))
                .Subscribe();
        }

        private string _greeting;
        public string Greeting {
            get => _greeting;
            set => this.RaiseAndSetIfChanged(ref _greeting, value);
        }

        private static readonly string[] Adjectives = {
            "reactive",
            "expressive",
            "clear",
            "concurrent"
        };
    }
}

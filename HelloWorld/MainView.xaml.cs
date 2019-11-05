using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Threading;

namespace ReactiveAvalonia.HelloWorld {

    // http://avaloniaui.net/docs/reactiveui/activation#activation-example
    // https://reactiveui.net/docs/handbook/data-binding/avalonia
    public class MainView : ReactiveWindow<MainViewModel> {

        // https://reactiveui.net/docs/handbook/data-binding/avalonia
        private TextBlock GreetingLabel => this.FindControl<TextBlock>("GreetingLabel");

        public MainView() {
            ViewModel = new MainViewModel();

            //https://reactiveui.net/docs/handbook/when-activated/#views
            this
                .WhenActivated(
                    disposables => {
                        // Jut log the View's activation
                        Console.WriteLine(
                            $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                            "View activated\n");

                        // Just log the View's deactivation
                        Disposable
                            .Create(
                                () =>
                                    Console.WriteLine(
                                        $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                                        "View deactivated"))
                            .DisposeWith(disposables);

                        // https://reactiveui.net/docs/handbook/data-binding/
                        this
                            .OneWayBind(ViewModel, vm => vm.Greeting, v => v.GreetingLabel.Text)
                            .DisposeWith(disposables);
                    });

            InitializeComponent();
        }

        private void InitializeComponent() {
            // https://reactiveui.net/docs/handbook/data-binding/avalonia
            AvaloniaXamlLoader.Load(this);
        }
    }
}

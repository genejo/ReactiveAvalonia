using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;

namespace ReactiveAvalonia.HelloWorld {
    public class MainView : ReactiveWindow<MainViewModel> {
        private TextBlock GreetingLabel => this.FindControl<TextBlock>("GreetingLabel");

        public MainView() {
            ViewModel = new MainViewModel();

            this
                .WhenActivated(
                    d => {
                        this
                            .OneWayBind(ViewModel, vm => vm.Greeting, v => v.GreetingLabel.Text)
                            .DisposeWith(d);
                    });

            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

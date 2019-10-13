using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System;

namespace ReactiveAvalonia
{
    public class MainView : ReactiveWindow<MainViewModel> {
        private TextBlock _greetingLabel => this.FindControl<TextBlock>("GreetingLabel");

        public MainView() {
            ViewModel = new MainViewModel();

			this
                .WhenActivated(
                    d => {
                        //*
                        this
                            .OneWayBind(ViewModel, vm => vm.Greeting, v => v._greetingLabel.Text)
                            .DisposeWith(d);
                        /*/
                        this
                            .WhenAnyValue(x => x.ViewModel.Greeting)
                            .Subscribe(
                                x => {
                                    Console.WriteLine($"[v ]: Name is now {x}");
                                    Texty.Text = x;
                                })
                            .DisposeWith(disposables);
                        //*/
                    });
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
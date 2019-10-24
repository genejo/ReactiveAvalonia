using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Reactive.Linq;
using System.ComponentModel;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainView : ReactiveWindow<MainViewModel> {
        private TextBlock tblBuddyInfo => this.FindControl<TextBlock>("tblBuddyInfo");
        private TextBlock tblDecisionTimeLeft => this.FindControl<TextBlock>("tblDecisionTimeLeft");
        private Button btnStalkBuddy => this.FindControl<Button>("btnStalkBuddy");
        private Window wndMain => this.FindControl<Window>("wndMain");

        public MainView() {
            ViewModel = new MainViewModel();

            this
                .WhenActivated(
                    disposables => {
                        Console.WriteLine(
                            $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                            "View activated");

                        Disposable
                            .Create(
                                () => 
                                    Console.WriteLine(
                                        $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                                        "View deactivated"))
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(ViewModel, vm => vm.Remaining, v => v.tblDecisionTimeLeft.Text)
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(ViewModel, vm => vm.IsFetching, v => v.btnStalkBuddy.IsEnabled,
                                fetching => !fetching)
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(ViewModel, vm => vm.IsTimerPaused, v => v.btnStalkBuddy.Content,
                                paused => paused ? "Continue" : "Stalk buddy")
                            .DisposeWith(disposables);

                        this
                            .BindCommand(ViewModel, vm => vm.PerformCommand, v => v.btnStalkBuddy)
                            .DisposeWith(disposables);

                        Observable
                            .FromEventPattern(wndMain, nameof(wndMain.Closing))
                            .Subscribe(
                                _ => {
                                    Console.WriteLine(
                                        $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                                        "Main window closing...");
                                })
                            .DisposeWith(disposables);
                    });

            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

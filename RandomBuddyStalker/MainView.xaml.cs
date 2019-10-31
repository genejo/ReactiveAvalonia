using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainView : ReactiveWindow<MainViewModel> {
        private MainViewModel _vm;
        public MainView() {
            ViewModel = _vm = new MainViewModel();

            this
                .WhenActivated(
                    disposables => {
                        Console.WriteLine(
                            $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                            "View activated");

                        this
                            .OneWayBind(_vm, vm => vm.Remaining, v => v.tblDecisionTimeLeft.Text)
                            .DisposeWith(disposables);

                        this
                            .WhenAnyValue(v => v._vm.Remaining)
                            .Subscribe(
                                remaining => { 
                                    pbLeftRemainingTime.Value = remaining; 
                                    pbRightRemainingTime.Value = remaining; 
                                })
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(_vm, vm => vm.IsFetching, v => v.btnStalkBuddy.IsEnabled,
                                fetching => !fetching)
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(_vm, vm => vm.IsTimerPaused, v => v.btnStalkBuddy.Content,
                                paused => paused ? "Continue" : "Stalk buddy")
                            .DisposeWith(disposables);

                        this
                            .BindCommand(_vm, vm => vm.StalkOrContinueCommand, v => v.btnStalkBuddy)
                            .DisposeWith(disposables);

                        // https://reactiveui.net/docs/handbook/commands/#invoking-commands-in-an-observable-pipeline
                        Observable
                            .FromEventPattern(wndMain, nameof(wndMain.LayoutUpdated))
                            .Take(1)
                            .Select(x => Unit.Default)
                            .InvokeCommand(this, v => v.ViewModel.StalkOrContinueCommand)
                            .DisposeWith(disposables);

                        Disposable
                            .Create(
                                () => 
                                    Console.WriteLine(
                                        $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                                        "View deactivated"))
                            .DisposeWith(disposables);

                        // https://reactiveui.net/docs/handbook/events/#how-do-i-convert-my-own-c-events-into-observables
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

        private Window wndMain => this.FindControl<Window>("wndMain");
        private TextBlock tblBuddyInfo => this.FindControl<TextBlock>("tblBuddyInfo");
        private TextBlock tblDecisionTimeLeft => this.FindControl<TextBlock>("tblDecisionTimeLeft");
        private Button btnStalkBuddy => this.FindControl<Button>("btnStalkBuddy");
        private ProgressBar pbLeftRemainingTime => this.FindControl<ProgressBar>("pbLeftRemainingTime");
        private ProgressBar pbRightRemainingTime => this.FindControl<ProgressBar>("pbRightRemainingTime");
    }
}

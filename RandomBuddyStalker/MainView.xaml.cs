using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainView : ReactiveWindow<MainViewModel> {
        private MainViewModel _vm;
        public MainView() {
            ViewModel = _vm = new MainViewModel();

            this
                .WhenActivated(
                    disposables => {
                        // https://reactiveui.net/docs/handbook/data-binding/
                        this
                            .OneWayBind(_vm, vm => vm.BuddyName, v => v.tblBuddyName.Text)
                            .DisposeWith(disposables);

                        this
                            .OneWayBind(_vm, vm => vm.BuddyAvatarBitmap, v => v.imgAvatar.Source)
                            .DisposeWith(disposables);

                        // https://reactiveui.net/docs/handbook/commands/binding-commands
                        this
                            .BindCommand(_vm, vm => vm.StalkCommand, v => v.btnStalk)
                            .DisposeWith(disposables);
                        
                        this
                            .BindCommand(_vm, vm => vm.ContinueCommand, v => v.btnContinue)
                            .DisposeWith(disposables);

                        this
                            .WhenAnyValue(v => v._vm.IsTimerRunning)
                            .Do(running => {
                                btnStalk.IsVisible = running;
                                btnContinue.IsVisible = !running;                                
                                brdAvatar.IsVisible = !running;
                                if (!running)
                                    pbRemainingTime.Value = 0;
                            })
                            .Subscribe()
                            .DisposeWith(disposables);

                        // At the time of writing Avalonia UI control animations are not stable.
                        // For this reason we're manually implementing the progress bar animation.
                        // Disclaimer: self-contained animation code snippet to the  detriment of efficiency.
                        this
                            .WhenAnyObservable(v => v._vm.TriggeringTheTimer)
                            .Where(trigger => trigger == TimerTrigger.Start)
                            .Do(trigger => {
                                const int barDivisionsCount = 8;
                                int divisionTimeSpan = MainViewModel.DecisionTimeMilliseconds / (barDivisionsCount + 1);
                                int barDivisionLength = MainViewModel.DecisionTimeMilliseconds / barDivisionsCount;
                                Observable
                                    .Timer(
                                        TimeSpan.FromMilliseconds(0),
                                        TimeSpan.FromMilliseconds(divisionTimeSpan),
                                        RxApp.MainThreadScheduler)
                                    .TakeWhile(item => item <= barDivisionsCount && _vm.IsTimerRunning)
                                    .Subscribe(divisionsSoFar => {
                                        int remainingTime = (int)divisionsSoFar * barDivisionLength;
                                        pbRemainingTime.Value = remainingTime;
                                    });
                            })
                            .Subscribe()
                            .DisposeWith(disposables);

                        // TODO: move this to Hello World application
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

            pbRemainingTime.Maximum = MainViewModel.DecisionTimeMilliseconds;
        }

        private Window wndMain => this.FindControl<Window>("wndMain");
        private TextBlock tblBuddyName => this.FindControl<TextBlock>("tblBuddyName");
        private Button btnStalk => this.FindControl<Button>("btnStalk");
        private Button btnContinue => this.FindControl<Button>("btnContinue");
        private ProgressBar pbRemainingTime => this.FindControl<ProgressBar>("pbRemainingTime");
        private Border brdAvatar => this.FindControl<Border>("brdAvatar");
        private Image imgAvatar => this.FindControl<Image>("imgAvatar");
    }
}

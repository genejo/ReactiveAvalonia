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
                        Console.WriteLine(
                            $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                            "View activated" + '\n');

                        this
                            .OneWayBind(_vm, vm => vm.BuddyName, v => v.tblBuddyName.Text)
                            .DisposeWith(disposables);

                        this
                            .BindCommand(_vm, vm => vm.StalkCommand, v => v.btnStalk)
                            .DisposeWith(disposables);
                        
                        this
                            .BindCommand(_vm, vm => vm.ContinueCommand, v => v.btnContinue)
                            .DisposeWith(disposables);

                        this
                            .WhenAnyValue(v => v._vm.IsTimerRunning)
                            .Do(running => {
                                btnStalk.IsEnabled = running;
                                btnContinue.IsEnabled = !running;

                                if (!running) {
                                    Console.WriteLine(
                                        $"[v  {Thread.CurrentThread.ManagedThreadId}]: " +
                                        "Timer stopped");
                                }
                            })
                            .Subscribe()
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
            //imgAvatar.Source = new Bitmap("[path to image]/128.jpg");
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private Window wndMain => this.FindControl<Window>("wndMain");
        private TextBlock tblBuddyName => this.FindControl<TextBlock>("tblBuddyName");
        private TextBlock tblDecisionTimeLeft => this.FindControl<TextBlock>("tblDecisionTimeLeft");
        private Button btnStalk => this.FindControl<Button>("btnStalk");
        private Button btnContinue => this.FindControl<Button>("btnContinue");
        private ProgressBar pbLeftRemainingTime => this.FindControl<ProgressBar>("pbLeftRemainingTime");
        private ProgressBar pbRightRemainingTime => this.FindControl<ProgressBar>("pbRightRemainingTime");
        private Image imgAvatar => this.FindControl<Image>("imgAvatar");
    }
}

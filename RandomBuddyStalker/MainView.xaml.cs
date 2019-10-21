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
                    });

            InitializeComponent();
        }

        void OnWindowClosing(object sender, CancelEventArgs e) {
            Console.WriteLine(
                $"[v {Thread.CurrentThread.ManagedThreadId}]: " +
                "Main window closing...");
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

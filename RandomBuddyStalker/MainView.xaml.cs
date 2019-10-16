using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Reactive.Linq;

namespace ReactiveAvalonia.RandomBuddyStalker {
    public class MainView : ReactiveWindow<MainViewModel> {
        private TextBlock tblLeftBuddyInfo => this.FindControl<TextBlock>("tblLeftBuddyInfo");
        private TextBlock tblRightBuddyInfo => this.FindControl<TextBlock>("tblRightBuddyInfo");
        private Button btnStalkLeftBuddy => this.FindControl<Button>("btnStalkLeftBuddy");
        private Button btnStalkRightBuddy => this.FindControl<Button>("btnStalkRightBuddy");

        public MainView() {
            ViewModel = new MainViewModel();

            this
                .WhenActivated(
                    disposables => {
                        Console.WriteLine($"[v  { Thread.CurrentThread.ManagedThreadId}]: View activated");

                        Disposable
                            .Create(() => Console.WriteLine($"[v  {Thread.CurrentThread.ManagedThreadId}]: View deactivated"))
                            .DisposeWith(disposables);

                        Observable
                            .Interval(TimeSpan.FromSeconds(1))
                            .Take(1)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(
                                _ => {
                                    Console.WriteLine($"--[v  {Thread.CurrentThread.ManagedThreadId}]: {tblLeftBuddyInfo.Text}");
                                    Console.WriteLine($"--[v  {Thread.CurrentThread.ManagedThreadId}]: {tblRightBuddyInfo.Text}");
                                    Console.WriteLine($"--[v  {Thread.CurrentThread.ManagedThreadId}]: {btnStalkLeftBuddy.Name}");
                                    Console.WriteLine($"--[v  {Thread.CurrentThread.ManagedThreadId}]: {btnStalkRightBuddy.Name}");
                                },
                                err => Console.WriteLine($"error: {err}"),
                                () => { Console.WriteLine("Done with the introductions..."); })
                            .DisposeWith(disposables);
                    });

            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

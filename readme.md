# Reactive Avalonia

## Purpose

Self-contained multiplatform samples to help understanding: [ReactiveX](http://reactivex.io), [ReactiveUI](https://reactiveui.net), [Avalonia UI](https://avaloniaui.net). Featuring links to original reference material for each fresh chunk of code, e.g.

```cs
// https://reactiveui.net/docs/handbook/events/#how-do-i-convert-my-own-c-events-into-observables
Observable
    .FromEventPattern(wndMain, nameof(wndMain.Closing))
    .Subscribe(_ => Console.WriteLine("Main window closing..."))
    .DisposeWith(disposables);
```

## Samples

#### [Hello World](https://github.com/genejo/ReactiveAvalonia/tree/master/HelloWorld)

Uses a finite observable stream of timer ticks. At each tick a new
greeting is displayed. The sample can be a template for ReactiveUI + Avalonia
applications.

*Topics: View, ViewModel, (de)activation, timer, reactive property, type-safe bindings,
WhenAnyValue, observable timer, UI thread and schedulers, window event.*

<img  width="200" src="https://www.dropbox.com/s/ykhs4f322fwi7sx/HelloReactiveWorld_Trailer.gif?raw=1" />

#### [Random Buddy Stalker](https://github.com/genejo/ReactiveAvalonia/tree/master/RandomBuddyStalker)

Shows how to use async/await in a ReactiveUI context. It calls a
dummy [(but real)](https://reqres.in) RESTful API.

*Topics: command binding, ReactiveUI.Fody, Rx event pattern, WhenAnyObservable, async/await,
timeout, json, Flurl.*

<img  width="200" src="https://www.dropbox.com/s/bzt56v87b6hzpai/RandomBuddyStalker_Trailer.gif?raw=1" />

## Aims

<p align="center">
<img  width="320" src="https://www.dropbox.com/s/9pafnb2t1d591un/featuring_versatile_samples.png?raw=1" />
</p>

<p align="center">
<img  width="450" src="https://www.dropbox.com/s/a6v7hrdk1ufxp9s/featuring_links_to_explanations.png?raw=1" />
</p>

<p align="center">
<img  width="320" src="https://www.dropbox.com/s/s8k40fhgllyjcmy/featuring_multiplatform.png?raw=1" />
</p>

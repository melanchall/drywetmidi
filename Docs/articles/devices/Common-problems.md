---
uid: a_devices_commonproblems
---

# Common problems

## `StartCoroutine` can only be called from the main thread in Unity

Sometimes you want to start Unity coroutine in a handler of the [`EventReceived`](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint.EventReceived) event of [`InputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint). Your code can be executed on a separate thread in this case. It can happen because events are received by endpoint on a separate (system) thread.

But UI related things like call of `StartCoroutine` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related question on StackOverflow: [Catching and processing multiple keyboard inputs at once](https://stackoverflow.com/q/62750863)

## Updating Avalonia UI from `EndpointsWatcher`

Handlers of [`EndpointsWatcher`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher) should not update Avalonia controls or view models directly because watcher callbacks can run outside the UI thread. Keep a reference to your main view model in a bootstrap/service layer and marshal UI work through `Dispatcher.UIThread`:

```csharp
EndpointsWatcher.Instance.EndpointAdded += (_, e) =>
{
    Dispatcher.UIThread.Post(() =>
    {
        _mainViewModel.OnEndpointAdded(e.Endpoint);
    });
};

EndpointsWatcher.Instance.EndpointRemoved += (_, e) =>
{
    Dispatcher.UIThread.Post(() =>
    {
        _mainViewModel.OnEndpointRemoved(e.Endpoint); // remove by endpoint identity, not endpoint properties
    });
};
```

If you use Windows MIDI Services in Avalonia, initialize DryWetMIDI advanced API on a dedicated MTA thread before UI startup and use `Dispatcher.UIThread` only for the final UI update step.

## `InputEndpoint` declared as a local variable

If an instance of the [`InputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint) is declared as a local variable and you’ve subscribed to its [`EventReceived`](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint.EventReceived) event, the event handler won’t be called or you can get undefined behavior. For example, let’s look at this code:

```csharp
private void StartListening()
{
    var inputEndpoint = InputEndpoint.GetByName("My Device");
    inputEndpoint.EventReceived += OnEventReceived;
    inputEndpoint.StartEventsListening();
}

private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
{
    // Do something ...
}
```

What happens when the `StartListening` method exits? Right, all local variables are marked as ready for garbage collection, so they will be deleted within a short time span. So the `OnEventReceived` method becomes attached to a deleted entity (`inputEndpoint`) when the `StartListening` exited. In the best case you just won't get `OnEventReceived` called. In worst case you may get different strange things, see this issues:

* [InputEndpoint event listening crash](https://github.com/melanchall/drywetmidi/issues/262)
* [Crash when running in Unity on M2 MacBook](https://github.com/melanchall/drywetmidi/issues/267)

Input endpoint **must always** be stored in a class field:

```csharp
private InputEndpoint _inputEndpoint;

private void StartListening()
{
    _inputEndpoint = InputEndpoint.GetByName("My Device");
    _inputEndpoint.EventReceived += OnEventReceived;
    _inputEndpoint.StartEventsListening();
}
```

And don't forget to dispose the endpoint when you're done with it. Please read the [Input endpoint](xref:a_dev_input) article to learn more.

## `OutputEndpoint` declared as a local variable

The same story is with the [`OutputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint) class. If you declare an instance of `OutputEndpoint` as a local variable, you can get undefined behavior when trying to send MIDI events to it. For example:

```csharp
private void SendEvent()
{
    var outputEndpoint = OutputEndpoint.GetByName("My Device");
    outputEndpoint.SendEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)60));
}
```

`SendEvent` sends MIDI event to a device driver where it will be processed. If `outputEndpoint` collected by GC during sending an event to the driver, undefined behavior possible including app crash.

As with input endpoint, output endpoint **must always** be stored in a class field. Don't forget to dispose the endpoint when you're done with it. Please read the [Output endpoint](xref:a_dev_output) article to learn more.
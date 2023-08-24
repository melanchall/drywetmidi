---
uid: a_devices_commonproblems
---

# Common problems

## `StartCoroutine` can only be called from the main thread in Unity

Sometimes you want to start Unity coroutine in a handler of [EventReceived](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice.EventReceived) event of [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice). Your code can be executed on separate thread in these case. It can happen because of events are received by device on separate (system) thread.

But UI related things like call of `StartCoroutine` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related question on StackOverflow: [Catching and processing multiple keyboard inputs at once](https://stackoverflow.com/q/62750863)

## `InputDevice` declared as a local variable

If an instance of the [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice) is declared as a local variable and you’ve subscribed to its [EventReceived](xref:Melanchall.DryWetMidi.Multimedia.IInputDevice.EventReceived) event, the event handler won’t be called or you can get undefined behavior. For example, let’s look at this code:

```csharp
private void StartListening()
{
    var inputDevice = InputDevice.GetByName("My Device");
    inputDevice.EventReceived += OnEventReceived;
    inputDevice.StartEventsListening();
}

private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
{
    // Do something ...
}
```

What happens when the `StartListening` method exits? Right, all local variables are marked as ready for garbage collection, so they will be deleted within a short time span. So the `OnEventReceived` method becomes attached to a deleted entity (`inputDevice`) when the `StartListening` exited. In the best case you just won't get `OnEventReceived` called. In worst case you may get different strange things, see this issues:

* [InputDevice event listening crash](https://github.com/melanchall/drywetmidi/issues/262)
* [Crash when running in Unity on M2 MacBook](https://github.com/melanchall/drywetmidi/issues/267)

Input device **must** be stored in a class field:

```csharp
private InputDevice _inputDevice;

private void StartListening()
{
    _inputDevice = InputDevice.GetByName("My Device");
    _inputDevice.EventReceived += OnEventReceived;
    _inputDevice.StartEventsListening();
}
```

And don't forget to dispose the device when you're done with it. Please read the [Input device](xref:a_dev_input) article to learn more.
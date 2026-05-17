---
uid: a_devices_commonproblems
---

# Common problems

## `StartCoroutine` can only be called from the main thread in Unity

Sometimes you want to start Unity coroutine in a handler of the [EventReceived](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint.EventReceived) event of [InputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint). Your code can be executed on a separate thread in this case. It can happen because events are received by endpoint on a separate (system) thread.

But UI related things like call of `StartCoroutine` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related question on StackOverflow: [Catching and processing multiple keyboard inputs at once](https://stackoverflow.com/q/62750863)

## `InputEndpoint` declared as a local variable

If an instance of the [InputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint) is declared as a local variable and you’ve subscribed to its [EventReceived](xref:Melanchall.DryWetMidi.Multimedia.IInputEndpoint.EventReceived) event, the event handler won’t be called or you can get undefined behavior. For example, let’s look at this code:

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

Input endpoint **must** be stored in a class field:

```csharp
private InputEndpoint _inputEndpoint;

private void StartListening()
{
    _inputEndpoint = InputEndpoint.GetByName("My Device");
    _inputEndpoint.EventReceived += OnEventReceived;
    _inputEndpoint.StartEventsListening();
}
```

And don't forget to dispose of the endpoint when you're done with it. Please read the [Input endpoint](xref:a_dev_input) article to learn more.
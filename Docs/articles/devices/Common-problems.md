---
uid: a_devices_commonproblems
---

# Common problems

## StartCoroutine can only be called from the main thread in Unity

Sometimes you want to start Unity coroutine in a handler of [EventReceived](xref:Melanchall.DryWetMidi.Devices.IInputDevice.EventReceived) event of [InputDevice](xref:Melanchall.DryWetMidi.Devices.InputDevice). Your code can be executed on separate thread in these case. It can happen because of events are received by device on separate (system) thread.

But UI related things like call of `StartCoroutine` can be executed on UI thread only. You can use the solution from here: https://stackoverflow.com/a/56715254.

Related question on StackOverflow: [Catching and processing multiple keyboard inputs at once](https://stackoverflow.com/q/62750863)
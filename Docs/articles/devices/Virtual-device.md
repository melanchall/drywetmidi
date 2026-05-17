---
uid: a_dev_virtual
---

# Virtual device

> [!WARNING]
> Virtual devices API is a platform-specific one so please refer to the [Supported OS](xref:a_develop_supported_os) article to learn more. For Windows you can use products like [virtualMIDI SDK](https://www.tobias-erichsen.de/software/virtualmidi/virtualmidi-sdk.html) or similar to work with virtual MIDI ports programmatically. Be careful with the license of these products.

With DryWetMIDI you can programmatically create virtual MIDI devices with the specified name using [VirtualDevice.Create](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.Create(System.String)) method. In fact, virtual device is an [input](xref:a_dev_input) and an [output](xref:a_dev_output) devices paired together in a way that any MIDI event sent to the output device will be immediately transferred back from the virtual device and can be received by an application from its input subdevice.

Thus we have a [loopback](https://en.wikipedia.org/wiki/Loopback) device here. Loopback device is useful, for example, as an intermediate layer between an application and some software synthesizer. In this case:

1. you create virtual device, for example, named as _MyDevice_;
2. in the application you set _MyDevice_ as an output MIDI port, so the application will send MIDI data to the output subdevice of the virtual device;
3. in the software synthesizer you set _MyDevice_ as an input MIDI port.

So when you create a virtual device an input endpoint and an output one are created with the same name as the one specified on virtual device creation. Endpoints of a virtual device are available via [InputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.InputEndpoint) and [OutputEndpoint](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.OutputEndpoint) properties of the [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice) class. Of course you can use those endpoints separately as regular input and output endpoints:

```csharp
using System;
using Melanchall.DryWetMidi.Multimedia;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var virtualDevice = VirtualDevice.Create("MyDevice");
            Console.WriteLine($"Virtual device {virtualDevice} created with endpoints:");
            Console.WriteLine($"  input = {virtualDevice.InputEndpoint.Name}");
            Console.WriteLine($"  output = {virtualDevice.OutputEndpoint.Name}");

            var inputEndpoint = InputEndpoint.GetByName("MyDevice");
            Console.WriteLine($"Input endpoint {inputEndpoint.Name} got as regular input endpoint.");

            var outputEndpoint = OutputEndpoint.GetByName("MyDevice");
            Console.WriteLine($"Output endpoint {outputEndpoint.Name} got as regular output endpoint.");

            Console.ReadKey();
        }
    }
}
```

Output of the program:

```text
Virtual device Virtual device created with endpoints:
  input = MyDevice
  output = MyDevice
Input endpoint MyDevice got as regular input endpoint.
Output endpoint MyDevice got as regular output endpoint.
```

You can even combine virtual devices and [DevicesConnector](xref:a_dev_connector) to broadcast MIDI data to several applications (synthesizers, for example) at the same time:

```csharp
using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootDevice = VirtualDevice.Create("Root");
            rootDevice.InputEndpoint.StartEventsListening(); // Important, don't forget!

            var leafDevice1 = VirtualDevice.Create("Leaf1");
            leafDevice1.InputEndpoint.EventReceived += OnLeafEventReceived;

            var leafDevice2 = VirtualDevice.Create("Leaf2");
            leafDevice2.InputEndpoint.EventReceived += OnLeafEventReceived;

            var devicesConnector = rootDevice.InputEndpoint.Connect(
                leafDevice1.OutputEndpoint,
                leafDevice2.OutputEndpoint);
            leafDevice1.InputEndpoint.StartEventsListening();
            leafDevice2.InputEndpoint.StartEventsListening();

            var midiEvent = new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)60)
            {
                Channel = (FourBitNumber)5
            };

            Console.WriteLine($"Sending {midiEvent} event...");
            rootDevice.OutputEndpoint.SendEvent(midiEvent);

            Console.ReadKey();
        }

        private static void OnLeafEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var endpoint = (MidiEndpoint)sender;
            Console.WriteLine($"Event {e.Event} received on endpoint {endpoint.Name}.");
        }
    }
}
```

This program will print following lines:

```text
Sending Note On [5] (70, 60) event...
Event Note On [5] (70, 60) received on endpoint Leaf1.
Event Note On [5] (70, 60) received on endpoint Leaf2.
```

> [!WARNING]
> As with input and output endpoint you must always [dispose](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.Dispose) virtual device when you're done with it:
>
> ```csharp
> virtualDevice.Dispose();
> ```

You must not explicitly dispose of subdevices of a virtual device. More than that, calling `Dispose` on `virtualDevice.InputEndpoint` and `virtualDevice.OutputEndpoint` will throw an exception. But if you got references to the subdevices by regular methods (for example, by [InputEndpoint.GetByName](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint.GetByName(System.String))), you can call `Dispose` on those references of course.

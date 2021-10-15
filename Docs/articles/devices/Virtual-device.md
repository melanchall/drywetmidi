---
uid: a_dev_virtual
---

# Virtual device

> [!IMPORTANT]
> Virtual devices API available for macOS only. For Windows you can use products like [virtualMIDI SDK](https://www.tobias-erichsen.de/software/virtualmidi/virtualmidi-sdk.html) or similar to work with virtual MIDI ports programmatically. Be careful with license of these products.

With DryWetMIDI you can programmatically create virtual MIDI devices with the specified name using [VirtualDevice.Create](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.Create(System.String)) method. In fact virtual device is an [input](xref:a_dev_input) and an [output](xref:a_dev_output) devices paired together in a way that any MIDI event sent to the output device will be immediately transfered back from the virtual device and can be received by an application from its input subdevice.

Thus we have [loopback](https://en.wikipedia.org/wiki/Loopback) device here. Loopback device is useful, for example, as intermediate layer between an application and some software synthesizer. In this case:

1. you create virtual device, for example, named as _MyDevice_;
2. in the application you set _MyDevice_ as an output MIDI port, so the application will send MIDI data to the output subdevice of the virtual device;
3. in software synthesizer you set _MyDevice_ as an input MIDI port.

So when you create virtual device an input device and an output one are created with the same name as the one specified on virtual device creation. Subdevices of a virtual device are available via [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.InputDevice) and [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice.OutputDevice) properties of the [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice) class. Of course you can use those device separately as regular input and output devices:

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
            Console.WriteLine($"Virtual device {virtualDevice} created with subdevices:");
            Console.WriteLine($"  input = {virtualDevice.InputDevice.Name}");
            Console.WriteLine($"  output = {virtualDevice.OutputDevice.Name}");

            var inputDevice = InputDevice.GetByName("MyDevice");
            Console.WriteLine($"Input device {inputDevice.Name} got as regular input device.");

            var outputDevice = OutputDevice.GetByName("MyDevice");
            Console.WriteLine($"Output device {outputDevice.Name} got as regular output device.");

            Console.ReadKey();
        }
    }
}
```

Output of the program:

```
Virtual device Virtual device created with subdevices:
  input = MyDevice
  output = MyDevice
Input device MyDevice got as regular input device.
Output device MyDevice got as regular output device.
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
            rootDevice.InputDevice.StartEventsListening(); // Important, don't forget!

            var leafDevice1 = VirtualDevice.Create("Leaf1");
            leafDevice1.InputDevice.EventReceived += OnLeafEventReceived;

            var leafDevice2 = VirtualDevice.Create("Leaf2");
            leafDevice2.InputDevice.EventReceived += OnLeafEventReceived;

            var devicesConnector = rootDevice.InputDevice.Connect(
                leafDevice1.OutputDevice,
                leafDevice2.OutputDevice);
            leafDevice1.InputDevice.StartEventsListening();
            leafDevice2.InputDevice.StartEventsListening();

            var midiEvent = new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)60)
            {
                Channel = (FourBitNumber)5
            };

            Console.WriteLine($"Sending {midiEvent} event...");
            rootDevice.OutputDevice.SendEvent(midiEvent);

            Console.ReadKey();
        }

        private static void OnLeafEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var device = (MidiDevice)sender;
            Console.WriteLine($"Event {e.Event} received on device {device.Name}.");
        }
    }
}
```

This program will print following lines:

```
Sending Note On [5] (70, 60) event...
Event Note On [5] (70, 60) received on device Leaf1.
Event Note On [5] (70, 60) received on device Leaf2.
```

As with input and output device you should always [dispose](xref:Melanchall.DryWetMidi.Multimedia.MidiDevice.Dispose) virtual device when you're done with it:

```csharp
virtualDevice.Dispose();
```

You must not explicitly dispose subdevices of a virtual device. More than that calling `Dispose` on `virtualDevice.InputDevice` and `virtualDevice.OutputDevice` will throw an exception. But if you got references to the subdevices by regular methods (for example, by [InputDevice.GetByName](xref:Melanchall.DryWetMidi.Multimedia.InputDevice.GetByName(System.String))), you can call `Dispose` on that references of course.
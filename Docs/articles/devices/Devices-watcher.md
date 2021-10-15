---
uid: a_dev_watcher
---

# Devices watcher

> [!IMPORTANT]
> Watching devices available for macOS only.

DryWetMIDI allows to track whether a MIDI device added to or removed from the system. There is [DevicesWatcher](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher) class for that purpose. The class is singleton and you can get the instance with [Instance](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher.Instance) property.

`DevicesWatcher` provides two events: [DeviceAdded](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher.DeviceAdded) and [DeviceRemoved](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher.DeviceRemoved). First one will be fired when a MIDI device is added to the system, and second one - when a device removed from it. You can then cast device instance from the events arguments to [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice) or [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice). See following sample program:

```csharp
using System;
using System.Threading;
using Melanchall.DryWetMidi.Multimedia;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            DevicesWatcher.Instance.DeviceAdded += OnDeviceAdded;
            DevicesWatcher.Instance.DeviceRemoved += OnDeviceRemoved;

            Console.WriteLine("Adding device...");
            var virtualDevice = VirtualDevice.Create("MyDevice");

            Thread.Sleep(500); // to get system time to see new device

            Console.WriteLine("Removing device...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnDeviceRemoved(object sender, DeviceAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Device removed: {e.Device.GetType()}");
        }

        private static void OnDeviceAdded(object sender, DeviceAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Device added: {e.Device.GetType()} ({e.Device.Name})");
        }
    }
}
```

Running the program we'll see following output:

```
Adding device...
Device added: Melanchall.DryWetMidi.Multimedia.InputDevice (MyDevice)
Device added: Melanchall.DryWetMidi.Multimedia.OutputDevice (MyDevice)
Removing device...
Device removed: Melanchall.DryWetMidi.Multimedia.InputDevice
Device removed: Melanchall.DryWetMidi.Multimedia.OutputDevice
```

When device is added you can immediately interact with it using an instance from the `DeviceAdded` event's arguments. But an instance from the `DeviceRemoved` event's arguments is non-interactable, because device is removed and doesn't exist in the system anymore. Any attempt to call methods or properties on that instance will throw an exception:

```csharp
using System;
using System.Threading;
using Melanchall.DryWetMidi.Multimedia;

namespace DwmExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            DevicesWatcher.Instance.DeviceRemoved += OnDeviceRemoved;

            var virtualDevice = VirtualDevice.Create("MyDevice");
            Thread.Sleep(500); // to get system time to see new device

            Console.WriteLine("Removing device...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnDeviceRemoved(object sender, DeviceAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Device removed. Getting its name...");
            var deviceName = e.Device.Name;
        }
    }
}
```

The progrum will be crashed with:

```
Removing device...
Device removed. Getting its name...
Unhandled exception. System.InvalidOperationException: Operation can't be performed on removed device.
```

But you can compare device instances via `Equals` to know whether two instances of `MidiDevice` are equal or not. Following example shows how you can get the name of a removed device via info about devices stored at the start of the program:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Multimedia;

namespace DwmExamples
{
    class Program
    {
        private static Dictionary<MidiDevice, string> _devicesNames;

        static void Main(string[] args)
        {
            DevicesWatcher.Instance.DeviceRemoved += OnDeviceRemoved;

            var virtualDevice = VirtualDevice.Create("MyDevice");
            Thread.Sleep(500); // to get system time to see new device

            _devicesNames = InputDevice.GetAll()
                .OfType<MidiDevice>()
                .Concat(OutputDevice.GetAll())
                .ToDictionary(d => d, d => d.Name);

            Console.WriteLine("Removing device...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnDeviceRemoved(object sender, DeviceAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Device removed. Getting its name...");
            var deviceName = _devicesNames[e.Device];
            Console.WriteLine($"Name is {deviceName}");
        }
    }
}
```

Output is:

```
Removing device...
Device removed. Getting its name...
Name is MyDevice
Device removed. Getting its name...
Name is MyDevice
```

Device instances comparing can be useful in programs with GUI where you need update the list of available devices. So when a device is added, you just add it to the list. When some device is removed, you find corresponding item in the current list via `Equals` on device instances and remove that item.

> [!IMPORTANT]
> Checking for devices equality supported for macOS only. On Windows any call of `Equals` will just compare references.
---
uid: a_dev_watcher
---

# Endpoints watcher

> [!WARNING]
> <os-specific-api Endpoints watching API/>
> 
> <advanced-windows-api/>

DryWetMIDI allows to track whether a MIDI endpoint is added to or removed from the system. There is the [`EndpointsWatcher`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher) class for that purpose. The class is singleton and you can get the instance with [`Instance`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher.Instance) property.

`EndpointsWatcher` provides two events: [`EndpointAdded`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher.EndpointAdded) and [`EndpointRemoved`](xref:Melanchall.DryWetMidi.Multimedia.EndpointsWatcher.EndpointRemoved). First one will be fired when a MIDI endpoint is added to the system, and second one – when a MIDI endpoint is removed from it. You can then cast a device instance from the event's arguments to [`InputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.InputEndpoint) or [`OutputEndpoint`](xref:Melanchall.DryWetMidi.Multimedia.OutputEndpoint). See following sample program:

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
            EndpointsWatcher.Instance.EndpointAdded += OnEndpointAdded;
            EndpointsWatcher.Instance.EndpointRemoved += OnEndpointRemoved;

            Console.WriteLine("Adding device...");
            var virtualDevice = VirtualDevice.Create("MyDevice");

            Thread.Sleep(500); // to get system time to see new device

            Console.WriteLine("Removing device...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnEndpointRemoved(object sender, EndpointAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Endpoint removed: {e.Endpoint.GetType().Name}");
        }

        private static void OnEndpointAdded(object sender, EndpointAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Endpoint added: {e.Endpoint.GetType().Name} ({e.Endpoint.Name})");
        }
    }
}
```

Running the program we'll see following output:

```text
Adding device...
Endpoint added: InputEndpoint (MyDevice)
Endpoint added: OutputEndpoint (MyDevice)
Removing device...
Endpoint removed: InputEndpoint
Endpoint removed: OutputEndpoint
```

When an endpoint is added you can immediately interact with it using an instance from the `EndpointAdded` event's arguments. But an instance from the `EndpointRemoved` event's arguments is non-interactable, because the endpoint is removed and doesn't exist in the system anymore. Any attempt to use its properties on that instance will throw an exception:

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
            EndpointsWatcher.Instance.EndpointRemoved += OnEndpointRemoved;

            var virtualDevice = VirtualDevice.Create("MyDevice");
            Thread.Sleep(500); // to get system time to see new device

            Console.WriteLine("Removing endpoint...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnEndpointRemoved(object sender, EndpointAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Endpoint removed. Getting its name...");
            var endpointName = e.Endpoint.Name;
        }
    }
}
```

The program will be crashed with:

```text
Removing endpoint...
Endpoint removed. Getting its name...
Unhandled exception. System.InvalidOperationException: Operation can't be performed on removed endpoint.
```

You can compare endpoint instances via `Equals` to know whether two instances of `MidiEndpoint` are equal or not. Following example shows how you can get the name of a removed endpoint via info about endpoints stored at the start of the program:

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
            EndpointsWatcher.Instance.EndpointRemoved += OnEndpointRemoved;

            var virtualDevice = VirtualDevice.Create("MyDevice");
            Thread.Sleep(500); // to get system time to see new device

            _devicesNames = InputDevice.GetAll()
                .OfType<MidiDevice>()
                .Concat(OutputDevice.GetAll())
                .ToDictionary(d => d, d => d.Name);

            Console.WriteLine("Removing endpoint...");
            virtualDevice.Dispose();

            Console.ReadKey();
        }

        private static void OnEndpointRemoved(object sender, EndpointAddedRemovedEventArgs e)
        {
            Console.WriteLine($"Endpoint removed. Getting its name...");
            var endpointName = _devicesNames[e.Endpoint];
            Console.WriteLine($"Name is {endpointName}");
        }
    }
}
```

Output is:

```text
Removing endpoint...
Endpoint removed. Getting its name...
Name is MyDevice
Endpoint removed. Getting its name...
Name is MyDevice
```

Endpoint instances comparison can be useful in programs with GUI where you need to update the list of available endpoints. So when an endpoint is added, you just add it to the list. When some endpoint is removed, you find the corresponding item in the current list via `Equals` on endpoint instances and remove that item.

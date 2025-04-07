using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    [Platform("MacOsX")]
    public sealed class DevicesWatcherTests
    {
        #region Test methods

        [Test]
        public void CheckDeviceAddedRemoved()
        {
            var addedDevices = new List<MidiDevice>();
            var removedDevices = new List<MidiDevice>();

            EventHandler<DeviceAddedRemovedEventArgs> addedHandler = (_, e) => addedDevices.Add(e.Device);
            DevicesWatcher.Instance.DeviceAdded += addedHandler;

            EventHandler<DeviceAddedRemovedEventArgs> removedHandler = (_, e) => removedDevices.Add(e.Device);
            DevicesWatcher.Instance.DeviceRemoved += removedHandler;

            var deviceName = "VD7";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added = WaitOperations.Wait(() => addedDevices.Count >= 2, timeout);
                ClassicAssert.IsTrue(added, $"Devices weren't added for [{timeout}].");

                ClassicAssert.AreEqual(2, addedDevices.Count, $"Invalid count of added devices ({string.Join(", ", addedDevices.Select(d => $"{d.Context}"))}).");

                var firstAddedDevice = addedDevices.First();
                ClassicAssert.IsInstanceOf<InputDevice>(firstAddedDevice, "Invalid type of the first added device.");
                ClassicAssert.AreEqual(deviceName, firstAddedDevice.Name, "Invalid name of the first added device.");

                var lastAddedDevice = addedDevices.Last();
                ClassicAssert.IsInstanceOf<OutputDevice>(lastAddedDevice, "Invalid type of the last added device.");
                ClassicAssert.AreEqual(deviceName, lastAddedDevice.Name, "Invalid name of the last added device.");
            }

            var removed = WaitOperations.Wait(() => removedDevices.Count >= 2, timeout);
            ClassicAssert.IsTrue(removed, $"Devices weren't removed for [{timeout}].");

            ClassicAssert.AreEqual(2, removedDevices.Count, "Invalid count of removed devices.");

            var firstRemovedDevice = removedDevices.First();
            ClassicAssert.IsInstanceOf<InputDevice>(firstRemovedDevice, "Invalid type of the first removed device.");

            var lastRemovedDevice = removedDevices.Last();
            ClassicAssert.IsInstanceOf<OutputDevice>(lastRemovedDevice, "Invalid type of the last removed device.");

            DevicesWatcher.Instance.DeviceAdded -= addedHandler;
            DevicesWatcher.Instance.DeviceRemoved -= removedHandler;
        }

        #endregion
    }
}

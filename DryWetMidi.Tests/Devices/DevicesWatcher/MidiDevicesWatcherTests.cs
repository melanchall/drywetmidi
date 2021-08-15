using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("MacOsX")]
    public sealed class MidiDevicesWatcherTests
    {
        #region Test methods

        [Test]
        public void CheckDeviceAddedRemoved()
        {
            var addedDevices = new List<MidiDevice>();
            var removedDevices = new List<MidiDevice>();

            MidiDevicesWatcher.Instance.DeviceAdded += (_, e) => addedDevices.Add(e.Device);
            MidiDevicesWatcher.Instance.DeviceRemoved += (_, e) => removedDevices.Add(e.Device);

            var deviceName = "AAA";
            var timeout = TimeSpan.FromSeconds(5);

            using (var virtualDevice = VirtualDevice.Create(deviceName))
            {
                var added = WaitOperations.Wait(() => addedDevices.Count >= 2, timeout);
                Assert.IsTrue(added, $"Devices weren't added for [{timeout}].");

                Assert.AreEqual(2, addedDevices.Count, "Invalid count of added devices.");

                var firstAddedDevice = addedDevices.First();
                Assert.IsInstanceOf<InputDevice>(firstAddedDevice, "Invalid type of the first added device.");
                Assert.AreEqual(deviceName, firstAddedDevice.Name, "Invalid name of the first added device.");

                var lastAddedDevice = addedDevices.Last();
                Assert.IsInstanceOf<OutputDevice>(lastAddedDevice, "Invalid type of the last added device.");
                Assert.AreEqual(deviceName, lastAddedDevice.Name, "Invalid name of the last added device.");
            }

            var removed = WaitOperations.Wait(() => removedDevices.Count >= 2, timeout);
            Assert.IsTrue(removed, $"Devices weren't removed for [{timeout}].");

            Assert.AreEqual(2, removedDevices.Count, "Invalid count of removed devices.");

            var firstRemovedDevice = removedDevices.First();
            Assert.IsInstanceOf<InputDevice>(firstRemovedDevice, "Invalid type of the first removed device.");

            var lastRemovedDevice = removedDevices.Last();
            Assert.IsInstanceOf<OutputDevice>(lastRemovedDevice, "Invalid type of the last removed device.");
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Test methods

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceSupportedProperties_Mac()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    OutputDeviceProperty.Product,
                    OutputDeviceProperty.Manufacturer,
                    OutputDeviceProperty.DriverVersion,
                    OutputDeviceProperty.UniqueId,
                    OutputDeviceProperty.DriverOwner,
                },
                OutputDevice.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_Product_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputProduct", outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_Manufacturer_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputManufacturer", outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_DriverVersion_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual(200, outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_Technology_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is supported.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_UniqueId_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_VoicesNumber_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is supported.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_NotesNumber_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is supported.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_Channels_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is supported.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_Options_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is supported.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetOutputDeviceProperty_DriverOwner_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputDriverOwner", outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckOutputDevicesEquality_ViaEquals_SameDevices_Mac()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.AreEqual(outputDevice1, outputDevice2, "Devices are not equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckOutputDevicesEquality_ViaEquals_DifferentDevices_Mac()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.AreNotEqual(outputDevice1, outputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckOutputDevicesEquality_ViaOperator_SameDevices_Mac()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.IsTrue(outputDevice1 == outputDevice2, "Devices are not equal via equality.");
            Assert.IsFalse(outputDevice1 != outputDevice2, "Devices are not equal via inequality.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckOutputDevicesEquality_ViaOperator_DifferentDevices_Mac()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.IsFalse(outputDevice1 == outputDevice2, "Devices are equal via equality.");
            Assert.IsTrue(outputDevice1 != outputDevice2, "Devices are equal via inequality.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedOutputDeviceAccess_Name()
        {
            var outputDevice = GetRemovedOutputDevice();
            Assert.Throws<InvalidOperationException>(
                () => { var name = outputDevice.Name; },
                "Can get name of removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedOutputDeviceAccess_Property()
        {
            var outputDevice = GetRemovedOutputDevice();
            Assert.Throws<InvalidOperationException>(
                () => { var name = outputDevice.GetProperty(OutputDeviceProperty.Product); },
                "Can get property value of removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedOutputDeviceAccess_SendEvent()
        {
            var outputDevice = GetRemovedOutputDevice();
            Assert.Throws<InvalidOperationException>(
                () => outputDevice.SendEvent(new NoteOnEvent()),
                "Can send event via removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void OutputDeviceToString_AddedDevice()
        {
            var outputDevice = GetAddedOutputDevice();
            Assert.AreEqual("Output device (from 'Device added' notification)", outputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void OutputDeviceToString_RemovedDevice()
        {
            var outputDevice = GetRemovedOutputDevice();
            Assert.AreEqual("Output device (from 'Device removed' notification)", outputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void OutputDeviceToString_VirtualDevice()
        {
            var outputDevice = GetVirtualDeviceOutputDevice();
            Assert.AreEqual("Output device (subdevice of a virtual device)", outputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void FindOutputDeviceInDictionary()
        {
            var label = "X";
            var dictionary = new Dictionary<MidiDevice, string>
            {
                [OutputDevice.GetByName(MidiDevicesNames.DeviceA)] = label
            };

            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsTrue(dictionary.TryGetValue(outputDevice, out var value), "Failed to find device in dictionary.");
            Assert.AreEqual(label, value, "Device label is invalid.");
        }

        #endregion

        #region Private methods

        private static OutputDevice GetRemovedOutputDevice()
        {
            OutputDevice outputDevice = null;

            EventHandler<DeviceAddedRemovedEventArgs> handler = (_, e) => outputDevice = outputDevice ?? (e.Device as OutputDevice);
            DevicesWatcher.Instance.DeviceRemoved += handler;

            using (var virtualDevice = VirtualDevice.Create("VD6")) { }

            var timeout = TimeSpan.FromSeconds(5);
            var removed = WaitOperations.Wait(() => outputDevice != null, timeout);
            Assert.IsTrue(removed, $"Device wasn't removed for [{timeout}].");

            DevicesWatcher.Instance.DeviceRemoved -= handler;
            WaitOperations.Wait(1000);

            return outputDevice;
        }

        private static OutputDevice GetAddedOutputDevice()
        {
            OutputDevice outputDevice = null;

            EventHandler<DeviceAddedRemovedEventArgs> handler = (_, e) => outputDevice = outputDevice ?? (e.Device as OutputDevice);
            DevicesWatcher.Instance.DeviceAdded += handler;

            using (var virtualDevice = VirtualDevice.Create("VD5"))
            {
                var timeout = TimeSpan.FromSeconds(5);
                var added = WaitOperations.Wait(() => outputDevice != null, timeout);
                Assert.IsTrue(added, $"Device wasn't added for [{timeout}].");
            }

            DevicesWatcher.Instance.DeviceAdded -= handler;
            WaitOperations.Wait(1000);

            return outputDevice;
        }

        private static OutputDevice GetVirtualDeviceOutputDevice()
        {
            var result = default(OutputDevice);

            using (var virtualDevice = VirtualDevice.Create("VD4"))
            {
                result = virtualDevice.OutputDevice;
            }

            WaitOperations.Wait(1000);

            return result;
        }

        #endregion
    }
}

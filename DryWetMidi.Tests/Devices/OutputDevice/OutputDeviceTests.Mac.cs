using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
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

        #endregion
    }
}

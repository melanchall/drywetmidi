using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("MacOsX")]
    public sealed class OutputDevicePropertiesTests_Mac
    {
        #region Test methods

        [Test]
        public void GetOutputDeviceSupportedProperties()
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
        public void GetOutputDeviceProperty_Product()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputProduct", outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        public void GetOutputDeviceProperty_Manufacturer()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputManufacturer", outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        public void GetOutputDeviceProperty_DriverVersion()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual(200, outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        public void GetOutputDeviceProperty_Technology()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_UniqueId()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_VoicesNumber()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_NotesNumber()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_Channels()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_Options()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_DriverOwner()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("OutputDriverOwner", outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}

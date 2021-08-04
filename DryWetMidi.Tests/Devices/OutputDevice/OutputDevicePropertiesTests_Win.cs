using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("Win")]
    public sealed class OutputDevicePropertiesTests_Win
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
                    OutputDeviceProperty.Technology,
                    OutputDeviceProperty.VoicesNumber,
                    OutputDeviceProperty.NotesNumber,
                    OutputDeviceProperty.Channels,
                    OutputDeviceProperty.Options,
                },
                OutputDevice.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        public void GetOutputDeviceProperty_Product()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_Manufacturer()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_DriverVersion()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_Technology()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_UniqueId()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        public void GetOutputDeviceProperty_VoicesNumber()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_NotesNumber()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_Channels()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            object result;
            Assert.IsNotNull(result = outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is null.");
            CollectionAssert.IsNotEmpty((FourBitNumber[])result, "Channels are empty.");
        }

        [Test]
        public void GetOutputDeviceProperty_Options()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is null.");
        }

        [Test]
        public void GetOutputDeviceProperty_DriverOwner()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is supported.");
        }

        #endregion
    }
}

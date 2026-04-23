using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Test methods

        [Test]
        [MacOnly]
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
        [MacOnly]
        public void GetOutputDeviceProperty_Product_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("OutputProduct", outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_Manufacturer_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("OutputManufacturer", outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_DriverVersion_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual(200, outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_Technology_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_UniqueId_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_VoicesNumber_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_NotesNumber_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_Channels_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_Options_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is supported.");
        }

        [Test]
        [MacOnly]
        public void GetOutputDeviceProperty_DriverOwner_Mac()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("OutputDriverOwner", outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}

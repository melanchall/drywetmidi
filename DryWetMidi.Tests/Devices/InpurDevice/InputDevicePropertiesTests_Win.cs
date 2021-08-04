using System;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("Win")]
    public sealed class InputDevicePropertiesTests_Win
    {
        #region Test methods

        [Test]
        public void GetInputDeviceSupportedProperties()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    InputDeviceProperty.Product,
                    InputDeviceProperty.Manufacturer,
                    InputDeviceProperty.DriverVersion,
                },
                InputDevice.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        public void GetInputDeviceProperty_Product()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.Product), "Product is null.");
        }

        [Test]
        public void GetInputDeviceProperty_Manufacturer()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        public void GetInputDeviceProperty_DriverVersion()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        public void GetInputDeviceProperty_UniqueId()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        public void GetInputDeviceProperty_DriverOwner()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.Throws<ArgumentException>(() => inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is supported.");
        }

        #endregion
    }
}

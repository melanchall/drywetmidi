using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    [Platform("MacOsX")]
    public sealed class InputDevicePropertiesTests_Mac
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
                    InputDeviceProperty.UniqueId,
                    InputDeviceProperty.DriverOwner,
                },
                InputDevice.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        public void GetInputDeviceProperty_Product()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputProduct", inputDevice.GetProperty(InputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        public void GetInputDeviceProperty_Manufacturer()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputManufacturer", inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        public void GetInputDeviceProperty_DriverVersion()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual(100, inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        public void GetInputDeviceProperty_UniqueId()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        public void GetInputDeviceProperty_DriverOwner()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputDriverOwner", inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}

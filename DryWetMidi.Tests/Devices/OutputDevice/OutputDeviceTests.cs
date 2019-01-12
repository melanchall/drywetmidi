using System.Linq;
using Melanchall.DryWetMidi.Devices;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class OutputDeviceTests
    {
        #region Test methods
        
        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        [TestCase(MidiDevicesNames.MicrosoftGSWavetableSynth)]
        public void FindOutputDevice(string deviceName)
        {
            Assert.IsTrue(
                OutputDevice.GetAll().Any(d => d.Name == deviceName),
                $"There is no device '{deviceName}' in the system.");
        }

        [TestCase(MidiDevicesNames.DeviceA)]
        [TestCase(MidiDevicesNames.DeviceB)]
        [TestCase(MidiDevicesNames.MicrosoftGSWavetableSynth)]
        public void CheckOutputDeviceId(string deviceName)
        {
            var device = OutputDevice.GetByName(deviceName);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its name.");

            var deviceId = device.Id;
            device = OutputDevice.GetById(deviceId);
            Assert.IsNotNull(device, $"Unable to get device '{deviceName}' by its ID.");
            Assert.AreEqual(deviceName, device.Name, "Device retrieved by ID is not the same as retrieved by its name.");//
        }

        #endregion
    }
}

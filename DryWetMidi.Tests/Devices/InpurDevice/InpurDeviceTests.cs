using System.Linq;
using Melanchall.DryWetMidi.Devices;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class InpurDeviceTests
    {
        #region Test methods

        [Test]
        public void CheckInputDevices()
        {
            var allInputDevicesNames = InputDevice.GetAll().Select(d => d.Name).ToArray();

            foreach (var deviceName in new[] { MidiDevicesNames.DeviceA, MidiDevicesNames.DeviceB })
            {
                Assert.Contains(deviceName, allInputDevicesNames, $"There is no '{deviceName}' input MIDI device is the system.");
            }
        }

        #endregion
    }
}

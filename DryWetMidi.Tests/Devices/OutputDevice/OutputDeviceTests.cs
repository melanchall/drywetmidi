using System.Linq;
using Melanchall.DryWetMidi.Devices;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class OutputDeviceTests
    {
        #region Test methods

        [Test]
        public void CheckOutputDevices()
        {
            var allInputDevicesNames = OutputDevice.GetAll().Select(d => d.Name).ToArray();

            foreach (var deviceName in new[] { MidiDevicesNames.DeviceA, MidiDevicesNames.DeviceB, MidiDevicesNames.MicrosoftGSWavetableSynth })
            {
                Assert.Contains(deviceName, allInputDevicesNames, $"There is no '{deviceName}' output MIDI device is the system.");
            }
        }

        #endregion
    }
}

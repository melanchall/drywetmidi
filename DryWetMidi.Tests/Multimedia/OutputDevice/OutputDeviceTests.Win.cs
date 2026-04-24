using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Constants

        public const string MicrosoftGsWavetableSynth = "Microsoft GS Wavetable Synth";

        #endregion

        #region Test methods

        // TODO: failed on WMS enabled
        // [Test]
        [WinOnly]
        public void OutputDeviceIsInUse()
        {
            using (var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputDevice1.SendEvent(new NoteOnEvent());

                using (var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    ClassicAssert.Throws<MidiDeviceException>(() => outputDevice2.SendEvent(new NoteOnEvent()));
                }
            }
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceSupportedProperties_Win()
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
        [WinOnly]
        public void GetOutputDeviceProperty_Product_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_Manufacturer_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_DriverVersion_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_Technology_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_UniqueId_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_VoicesNumber_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_NotesNumber_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_Channels_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            object result;
            ClassicAssert.IsNotNull(result = outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is null.");
            CollectionAssert.IsNotEmpty((FourBitNumber[])result, "Channels are empty.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_Options_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is null.");
        }

        [Test]
        [WinOnly]
        public void GetOutputDeviceProperty_DriverOwner_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is supported.");
        }

        [MultimediaTestRetry]
        [Test]
        public void SendEvent_SysEx_SysExBufferSettings(
            [Values(32, 64, 128, 256, 1024, 4096)] int bufferSize,
            [Values(2, 4, 10, 100)] int buffersCount)
        {
            var bytes = Enumerable
                .Range(0, 10000)
                .Select(_ => (byte)DryWetMidi.Common.Random.Instance.Next(127))
                .Concat(new byte[] { 0xF7 })
                .ToArray();

            SendEvents(
                new[] { new NormalSysExEvent(bytes) },
                setupInputDevice: inputDevice =>
                {
                    inputDevice.SysExBufferSize = bufferSize;
                    inputDevice.SysExBuffersCount = buffersCount;
                });
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynth()
        {
            using (var outputDevice = OutputDevice.GetByName(MicrosoftGsWavetableSynth))
            {
                try
                {
                    outputDevice.PrepareForEventsSending();
                    outputDevice.SendEvent(new NoteOnEvent());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to work with the synth: {ex.Message}");
                }
            }
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynthEquality_SameDevices()
        {
            var outputDevice1 = OutputDevice.GetByName(MicrosoftGsWavetableSynth);
            var outputDevice2 = OutputDevice.GetByName(MicrosoftGsWavetableSynth);
            ClassicAssert.AreEqual(outputDevice1, outputDevice2, "Devices are not equal.");
        }

        [Test]
        [WinOnly]
        public void CheckMicrosoftGsWavetableSynthEquality_DifferentDevices()
        {
            var outputDevice1 = OutputDevice.GetByName(MicrosoftGsWavetableSynth);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreNotEqual(outputDevice1, outputDevice2, "Devices are equal.");
        }

        #endregion
    }
}

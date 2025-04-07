using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class OutputDeviceTests
    {
        #region Test methods

        [Test]
        [Platform("Win")]
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
        [Platform("Win")]
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
        [Platform("Win")]
        public void GetOutputDeviceProperty_Product_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Product), "Product is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_Manufacturer_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_DriverVersion_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.DriverVersion), "Driver version is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_Technology_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Technology), "Technology is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_UniqueId_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_VoicesNumber_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.VoicesNumber), "Voices number is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_NotesNumber_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.NotesNumber), "Notes number is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_Channels_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            object result;
            ClassicAssert.IsNotNull(result = outputDevice.GetProperty(OutputDeviceProperty.Channels), "Channels is null.");
            CollectionAssert.IsNotEmpty((FourBitNumber[])result, "Channels are empty.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_Options_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(outputDevice.GetProperty(OutputDeviceProperty.Options), "Options is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetOutputDeviceProperty_DriverOwner_Win()
        {
            var outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => outputDevice.GetProperty(OutputDeviceProperty.DriverOwner), "Driver owner is supported.");
        }

        [Test]
        [Platform("Win")]
        public void CheckOutputDevicesEquality_ViaEquals_SameDevices_Win()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.AreNotEqual(outputDevice1, outputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("Win")]
        public void CheckOutputDevicesEquality_ViaEquals_DifferentDevices_Win()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.AreNotEqual(outputDevice1, outputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("Win")]
        public void CheckOutputDevicesEquality_ViaOperator_SameDevices_Win()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.IsFalse(outputDevice1 == outputDevice2, "Devices are equal via equality.");
            ClassicAssert.IsTrue(outputDevice1 != outputDevice2, "Devices are equal via inequality.");
        }

        [Test]
        [Platform("Win")]
        public void CheckOutputDevicesEquality_ViaOperator_DifferentDevices_Win()
        {
            var outputDevice1 = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            var outputDevice2 = OutputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.IsFalse(outputDevice1 == outputDevice2, "Devices are equal via equality.");
            ClassicAssert.IsTrue(outputDevice1 != outputDevice2, "Devices are equal via inequality.");
        }

        #endregion
    }
}

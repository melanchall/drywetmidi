using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class InputDeviceTests
    {
        #region Test methods

        [Test]
        [Platform("Win")]
        public void ReceiveData_SinglepartSysExInSinglePacket_Win() => ReceiveData_Win(
            packets: new[]
            {
                new DataPacket(0xF0, 0x7F, 0x60, 0x40, 0xF7)
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [Platform("Win")]
        public void ReceiveData_MultipartSysExInOnePackage_Win() => ReceiveData_Win(
            packets: new[]
            {
                new DataPacket(0xF0, 0x7F, 0x60),
                new DataPacket(0x40, 0xF7)
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            },
            checkCheckpoints: false);

        [Test]
        [Platform("Win")]
        public void ReceiveData_MultipleMultipartSysExInOnePackage_Win() => ReceiveData_Win(
            packets: new[]
            {
                new DataPacket(0xF0, 0x7F, 0x60),
                new DataPacket(0x40, 0xF7),
                new DataPacket(0xF0, 0x5D, 0x6E),
                new DataPacket(0x7F, 0xF7)
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E, 0x7F, 0xF7 }),
            },
            checkCheckpoints: false);

        [Test]
        [Platform("Win")]
        public void ReceiveData_MultipleCompleteSysExInOnePackage_Win() => ReceiveData_Win(
            packets: new[]
            {
                new DataPacket(0xF0, 0x7F, 0x60, 0x40, 0xF7),
                new DataPacket(0xF0, 0x5D, 0x6E, 0x7F, 0xF7)
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E, 0x7F, 0xF7 }),
            });

        [Test]
        [Platform("Win")]
        public void InputDeviceIsInUse()
        {
            using (var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                inputDevice1.StartEventsListening();

                using (var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    ClassicAssert.Throws<MidiDeviceException>(() => inputDevice2.StartEventsListening());
                }
            }
        }

        [Test]
        [Platform("Win")]
        public void GetInputDeviceSupportedProperties_Win()
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
        [Platform("Win")]
        public void GetInputDeviceProperty_Product_Win()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.Product), "Product is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetInputDeviceProperty_Manufacturer_Win()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        [Platform("Win")]
        public void GetInputDeviceProperty_DriverVersion_Win()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [Platform("Win")]
        public void GetInputDeviceProperty_UniqueId_Win()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        [Platform("Win")]
        public void GetInputDeviceProperty_DriverOwner_Win()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.Throws<ArgumentException>(() => inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is supported.");
        }

        [Test]
        [Platform("Win")]
        public void CheckInputDevicesEquality_ViaEquals_SameDevices_Win()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.AreNotEqual(inputDevice1, inputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("Win")]
        public void CheckInputDevicesEquality_ViaEquals_DifferentDevices_Win()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.AreNotEqual(inputDevice1, inputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("Win")]
        public void CheckInputDevicesEquality_ViaOperator_SameDevices_Win()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            ClassicAssert.IsFalse(inputDevice1 == inputDevice2, "Devices are equal via equality.");
            ClassicAssert.IsTrue(inputDevice1 != inputDevice2, "Devices are equal via inequality.");
        }

        [Test]
        [Platform("Win")]
        public void CheckInputDevicesEquality_ViaOperator_DifferentDevices_Win()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            ClassicAssert.IsFalse(inputDevice1 == inputDevice2, "Devices are equal via equality.");
            ClassicAssert.IsTrue(inputDevice1 != inputDevice2, "Devices are equal via inequality.");
        }

        #endregion
    }
}

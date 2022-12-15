using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class InputDeviceTests
    {
        #region Test methods

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Single_SinglePacket() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60, 0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Single_SinglePackage() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Multiple_SinglePackage() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x40, 0xF7),
                    new DataPacket(0xF0, 0x5D, 0x6E),
                    new DataPacket(0x7F, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E, 0x7F, 0xF7 }),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Multiple_SinglePacket() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60, 0x40, 0xF7),
                    new DataPacket(0xF0, 0x5D, 0x6E, 0x7F, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E, 0x7F, 0xF7 }),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Single_MultiPackage() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60)),
                new DataPackage(
                    new DataPacket(0x40, 0xF7)),
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SysEx_Multiple_MultiPackage() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60)),
                new DataPackage(
                    new DataPacket(0x40, 0xF7)),
                new DataPackage(
                    new DataPacket(0xF0, 0x5D, 0x6E)),
                new DataPackage(
                    new DataPacket(0x7F, 0xF7)),
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E, 0x7F, 0xF7 }),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_SingleEventWithStatusByte() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x75, 0x56))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56)
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithStatusBytes() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x75, 0x56, 0x80, 0x55, 0x65, 0x90, 0x75, 0x56))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NoteOffEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_MultipleEventsWithRunningStatus() => ReceiveData(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x15, 0x56, 0x55, 0x65, 0x45, 0x60))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x15, (SevenBitNumber)0x56),
                new NoteOnEvent((SevenBitNumber)0x55, (SevenBitNumber)0x65),
                new NoteOnEvent((SevenBitNumber)0x45, (SevenBitNumber)0x60),
            });

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_LotOfEventsWithStatusBytes()
        {
            const int eventsCount = 3333;

            ReceiveData(
                packages: new[]
                {
                    new DataPackage(new DataPacket(Enumerable
                        .Range(0, eventsCount)
                        .SelectMany(i => new byte[] { 0x90, 0x75, 0x56 })
                        .ToArray()))
                },
                expectedEvents: Enumerable
                    .Range(0, eventsCount)
                    .Select(i => new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56))
                    .ToArray(),
                checkCheckpoints: false);
        }

        [Test]
        [Platform("MacOsX")]
        public void ReceiveData_UnexpectedRunningStatus()
        {
            var deviceName = MidiDevicesNames.DeviceA;

            var data = new byte[] { 0x56, 0x67, 0x45 };
            var indices = new[] { 0 };

            using (var dataSender = new DataSender(deviceName))
            using (var inputDevice = InputDevice.GetByName(deviceName))
            {
                Exception exception = null;

                inputDevice.ErrorOccurred += (_, e) => exception = e.Exception;
                inputDevice.StartEventsListening();

                dataSender.SendData(data, data.Length, indices, indices.Length);

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var errorOccurred = WaitOperations.Wait(() => exception != null, timeout);

                Assert.IsTrue(errorOccurred, $"Error was not occurred for [{timeout}].");
                Assert.IsInstanceOf(typeof(MidiDeviceException), exception, "Exception type is invalid");
                Assert.IsInstanceOf(typeof(UnexpectedRunningStatusException), exception.InnerException, "Inner exception type is invalid.");
            }

            WaitAfterReceiveData();
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceSupportedProperties_Mac()
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
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_Product_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputProduct", inputDevice.GetProperty(InputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_Manufacturer_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputManufacturer", inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_DriverVersion_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual(100, inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_UniqueId_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [Platform("MacOsX")]
        public void GetInputDeviceProperty_DriverOwner_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.AreEqual("InputDriverOwner", inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaEquals_SameDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.AreEqual(inputDevice1, inputDevice2, "Devices are not equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaEquals_DifferentDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.AreNotEqual(inputDevice1, inputDevice2, "Devices are equal.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaOperator_SameDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceA);

            Assert.IsTrue(inputDevice1 == inputDevice2, "Devices are not equal via equality.");
            Assert.IsFalse(inputDevice1 != inputDevice2, "Devices are not equal via inequality.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckInputDevicesEquality_ViaOperator_DifferentDevices_Mac()
        {
            var inputDevice1 = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            var inputDevice2 = InputDevice.GetByName(MidiDevicesNames.DeviceB);

            Assert.IsFalse(inputDevice1 == inputDevice2, "Devices are equal via equality.");
            Assert.IsTrue(inputDevice1 != inputDevice2, "Devices are equal via inequality.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedInputDeviceAccess_Name()
        {
            var inputDevice = GetRemovedInputDevice();
            Assert.Throws<InvalidOperationException>(
                () => { var name = inputDevice.Name; },
                "Can get name of removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedInputDeviceAccess_Property()
        {
            var inputDevice = GetRemovedInputDevice();
            Assert.Throws<InvalidOperationException>(
                () => { var name = inputDevice.GetProperty(InputDeviceProperty.Product); },
                "Can get property value of removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void CheckRemovedInputDeviceAccess_StartEventsListening()
        {
            var inputDevice = GetRemovedInputDevice();
            Assert.Throws<InvalidOperationException>(
                () => inputDevice.StartEventsListening(),
                "Can start events listening on removed device.");
        }

        [Test]
        [Platform("MacOsX")]
        public void InputDeviceToString_AddedDevice()
        {
            var inputDevice = GetAddedInputDevice();
            Assert.AreEqual("Input device (from 'Device added' notification)", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void InputDeviceToString_RemovedDevice()
        {
            var inputDevice = GetRemovedInputDevice();
            Assert.AreEqual("Input device (from 'Device removed' notification)", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void InputDeviceToString_VirtualDevice()
        {
            var inputDevice = GetVirtualDeviceInputDevice();
            Assert.AreEqual("Input device (subdevice of a virtual device)", inputDevice.ToString(), "Device string representation is invalid.");
        }

        [Test]
        [Platform("MacOsX")]
        public void FindInputDeviceInDictionary()
        {
            var label = "X";
            var dictionary = new Dictionary<MidiDevice, string>
            {
                [InputDevice.GetByName(MidiDevicesNames.DeviceA)] = label
            };

            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            Assert.IsTrue(dictionary.TryGetValue(inputDevice, out var value), "Failed to find device in dictionary.");
            Assert.AreEqual(label, value, "Device label is invalid.");
        }

        #endregion

        #region Private methods

        private static InputDevice GetRemovedInputDevice()
        {
            InputDevice inputDevice = null;

            EventHandler<DeviceAddedRemovedEventArgs> handler = (_, e) => inputDevice = inputDevice ?? (e.Device as InputDevice);
            DevicesWatcher.Instance.DeviceRemoved += handler;

            using (var virtualDevice = VirtualDevice.Create("VD3")) { }

            var timeout = TimeSpan.FromSeconds(5);
            var removed = WaitOperations.Wait(() => inputDevice != null, timeout);
            Assert.IsTrue(removed, $"Device wasn't removed for [{timeout}].");

            DevicesWatcher.Instance.DeviceRemoved -= handler;
            WaitOperations.Wait(1000);

            return inputDevice;
        }

        private static InputDevice GetAddedInputDevice()
        {
            InputDevice inputDevice = null;

            EventHandler<DeviceAddedRemovedEventArgs> handler = (_, e) => inputDevice = inputDevice ?? (e.Device as InputDevice);
            DevicesWatcher.Instance.DeviceAdded += handler;

            using (var virtualDevice = VirtualDevice.Create("VD2"))
            {
                var timeout = TimeSpan.FromSeconds(5);
                var added = WaitOperations.Wait(() => inputDevice != null, timeout);
                Assert.IsTrue(added, $"Device wasn't added for [{timeout}].");
            }

            DevicesWatcher.Instance.DeviceAdded -= handler;
            WaitOperations.Wait(1000);

            return inputDevice;
        }

        private static InputDevice GetVirtualDeviceInputDevice()
        {
            var result = default(InputDevice);

            using (var virtualDevice = VirtualDevice.Create("VD1"))
            {
                result = virtualDevice.InputDevice;
            }

            WaitOperations.Wait(1000);

            return result;
        }

        #endregion
    }
}

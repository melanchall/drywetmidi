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
    public sealed partial class InputDeviceTests
    {
        #region Test methods

        [Test]
        [MacOnly]
        public void ReceiveData_NoteOnAndSysExInOnePackage_Mac() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x75, 0x56),
                    new DataPacket(0xF0, 0x7F, 0x60, 0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [MacOnly]
        public void ReceiveData_NoteOnAndMultipartSysExInOnePackage_Mac_1() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x75, 0x56),
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x40, 0xF7 })
            });

        [Test]
        [MacOnly]
        public void ReceiveData_NoteOnAndMultipartSysExInOnePackage_Mac_2() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x90, 0x75, 0x56),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60, 0x90, 0x75, 0x56, 0x40, 0xF7 })
            });

        [Test]
        [MacOnly]
        public void ReceiveData_NoteOnAndMultipartSysExInOnePackage_DontWaitForCompleteSysExEvent_Mac_1() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0x90, 0x75, 0x56),
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
                new NormalSysExEvent(new byte[] { 0x7F, 0x60 })
            },
            waitForCompleteSysExEvent: false);

        [Test]
        [MacOnly]
        public void ReceiveData_NoteOnAndMultipartSysExInOnePackage_DontWaitForCompleteSysExEvent_Mac_2() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x90, 0x75, 0x56),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60 }),
                new NoteOnEvent((SevenBitNumber)0x75, (SevenBitNumber)0x56),
            },
            waitForCompleteSysExEvent: false);

        [Test]
        [MacOnly]
        public void ReceiveData_SinglepartSysExInSinglePacket_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipartSysExInOnePackage_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipartSysExInOnePackage_DontWaitForCompleteSysExEvent_Mac() => ReceiveData_Mac(
            packages: new[]
            {
                new DataPackage(
                    new DataPacket(0xF0, 0x7F, 0x60),
                    new DataPacket(0x40, 0xF7))
            },
            expectedEvents: new MidiEvent[]
            {
                new NormalSysExEvent(new byte[] { 0x7F, 0x60 })
            },
            waitForCompleteSysExEvent: false);

        [Test]
        [MacOnly]
        public void ReceiveData_MultipleMultipartSysExInOnePackage_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipleMultipartSysExInOnePackage_DontWaitForCompleteSysExEvent_Mac() => ReceiveData_Mac(
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
                new NormalSysExEvent(new byte[] { 0x7F, 0x60 }),
                new NormalSysExEvent(new byte[] { 0x5D, 0x6E }),
            },
            waitForCompleteSysExEvent: false);

        [Test]
        [MacOnly]
        public void ReceiveData_MultipleCompleteSysExInOnePackage_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipartSysExInMultiplePackages_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipleMultipartSysExsInMultiplePackage_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_SingleEventWithStatusByte_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipleEventsWithStatusBytes_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_MultipleEventsWithRunningStatus_Mac() => ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_LotOfEventsWithStatusBytes_Mac()
        {
            const int eventsCount = 3333;

            ReceiveData_Mac(
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
        [MacOnly]
        public void ReceiveData_UnexpectedRunningStatus_Mac()
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

                ClassicAssert.IsTrue(errorOccurred, $"Error was not occurred for [{timeout}].");
                ClassicAssert.IsInstanceOf(typeof(MidiDeviceException), exception, "Exception type is invalid");
                ClassicAssert.IsInstanceOf(typeof(UnexpectedRunningStatusException), exception.InnerException, "Inner exception type is invalid.");
            }

            WaitAfterReceiveData();
        }

        [Test]
        [MacOnly]
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
        [MacOnly]
        public void GetInputDeviceProperty_Product_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("InputProduct", inputDevice.GetProperty(InputDeviceProperty.Product), "Product is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputDeviceProperty_Manufacturer_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("InputManufacturer", inputDevice.GetProperty(InputDeviceProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputDeviceProperty_DriverVersion_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual(100, inputDevice.GetProperty(InputDeviceProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputDeviceProperty_UniqueId_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.IsNotNull(inputDevice.GetProperty(InputDeviceProperty.UniqueId), "Device unique ID is null.");
        }

        [Test]
        [MacOnly]
        public void GetInputDeviceProperty_DriverOwner_Mac()
        {
            var inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
            ClassicAssert.AreEqual("InputDriverOwner", inputDevice.GetProperty(InputDeviceProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}

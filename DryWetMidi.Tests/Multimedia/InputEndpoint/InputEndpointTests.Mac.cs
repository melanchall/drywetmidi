using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class InputEndpointTests
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
            var deviceName = MidiEndpoints.A;

            var data = new byte[] { 0x56, 0x67, 0x45 };
            var indices = new[] { 0 };

            using (var dataSender = new DataSender(deviceName))
            using (var inputEndpoint = DevicesUtilities.GetInputEndpoint(deviceName))
            {
                Exception exception = null;

                inputEndpoint.ErrorOccurred += (_, e) => exception = e.Exception;
                inputEndpoint.StartEventsListening();

                dataSender.SendData(data, data.Length, indices, indices.Length);

                var timeout = SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var errorOccurred = WaitOperations.Wait(() => exception != null, timeout);

                ClassicAssert.IsTrue(errorOccurred, $"Error was not occurred for [{timeout}].");
                ClassicAssert.IsInstanceOf(typeof(NativeApiException), exception, "Exception type is invalid");
                ClassicAssert.IsInstanceOf(typeof(UnexpectedRunningStatusException), exception.InnerException, "Inner exception type is invalid.");
            }

            WaitAfterReceiveData();
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointSupportedProperties_Mac()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    InputEndpointProperty.Product,
                    InputEndpointProperty.Manufacturer,
                    InputEndpointProperty.DriverVersion,
                    InputEndpointProperty.UniqueId,
                    InputEndpointProperty.DriverOwner,
                },
                InputEndpoint.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointProperty_Product_Mac()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreEqual("InputProduct", inputEndpoint.GetProperty(InputEndpointProperty.Product), "Product is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointProperty_Manufacturer_Mac()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreEqual("InputManufacturer", inputEndpoint.GetProperty(InputEndpointProperty.Manufacturer), "Manufacturer is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointProperty_DriverVersion_Mac()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreEqual(100, inputEndpoint.GetProperty(InputEndpointProperty.DriverVersion), "Driver version is invalid.");
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointProperty_UniqueId_Mac()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.IsNotNull(inputEndpoint.GetProperty(InputEndpointProperty.UniqueId), "Endpoint unique ID is null.");
        }

        [Test]
        [MacOnly]
        public void GetInputEndpointProperty_DriverOwner_Mac()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.AreEqual("InputDriverOwner", inputEndpoint.GetProperty(InputEndpointProperty.DriverOwner), "Driver owner is invalid.");
        }

        #endregion
    }
}

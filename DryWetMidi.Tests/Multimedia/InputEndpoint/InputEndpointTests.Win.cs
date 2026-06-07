using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class InputEndpointTests
    {
        #region Test methods

        [Test]
        [WinOnly]
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
        [WinOnly]
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
        [WinOnly]
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
        [WinOnly]
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

        // TODO: failed on WMS enabled
        // [Test]
        [WinOnly]
        public void InputEndpointIsInUse()
        {
            using (var inputEndpoint1 = InputEndpoint.GetByName(MidiEndpoints.A))
            {
                inputEndpoint1.StartEventsListening();

                using (var inputEndpoint2 = InputEndpoint.GetByName(MidiEndpoints.A))
                {
                    ClassicAssert.Throws<NativeApiException>(() => inputEndpoint2.StartEventsListening());
                }
            }
        }

        [Test]
        [WinOnly]
        public void GetInputEndpointSupportedProperties_Win()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    InputEndpointProperty.Product,
                    InputEndpointProperty.Manufacturer,
                    InputEndpointProperty.DriverVersion,
                },
                InputEndpoint.GetSupportedProperties(),
                "Invalid collection of supported properties.");
        }

        [Test]
        [WinOnly]
        public void GetInputEndpointProperty_Product_Win()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.IsNotNull(inputEndpoint.GetProperty(InputEndpointProperty.Product), "Product is null.");
        }

        [Test]
        [WinOnly]
        public void GetInputEndpointProperty_Manufacturer_Win()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.IsNotNull(inputEndpoint.GetProperty(InputEndpointProperty.Manufacturer), "Manufacturer is null.");
        }

        [Test]
        [WinOnly]
        public void GetInputEndpointProperty_DriverVersion_Win()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.IsNotNull(inputEndpoint.GetProperty(InputEndpointProperty.DriverVersion), "Driver version is invalid.");
        }

        // TODO: support on Windows
        [Test]
        [WinOnly]
        public void GetInputEndpointProperty_UniqueId_Win()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => inputEndpoint.GetProperty(InputEndpointProperty.UniqueId), "Device unique ID is supported.");
        }

        [Test]
        [WinOnly]
        public void GetInputEndpointProperty_DriverOwner_Win()
        {
            var inputEndpoint = InputEndpoint.GetByName(MidiEndpoints.A);
            ClassicAssert.Throws<ArgumentException>(() => inputEndpoint.GetProperty(InputEndpointProperty.DriverOwner), "Driver owner is supported.");
        }

        #endregion
    }
}

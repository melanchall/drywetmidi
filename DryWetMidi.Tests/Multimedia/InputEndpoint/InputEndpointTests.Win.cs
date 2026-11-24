using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
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

        #endregion
    }
}

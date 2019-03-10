using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestFixture]
    public sealed class SysExDataUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ReadSysExData_SampleDumpHeader()
        {
            var sampleDumpHeaderSysExData = ReadSysExData<SampleDumpHeaderSysExData>(new byte[]
            {
                0x7E,             // non-real time universal sys ex event
                0x01,             // channel
                0x01,             // 'sample dump header'
                0x03, 0x00,       // sample number
                0x10,             // sample format
                0x14, 0x31, 0x01, // sample period
                0x14, 0x03, 0x00, // sample length
                0x00, 0x00, 0x00, // loop start point
                0x14, 0x03, 0x00, // loop end point
                0x00,             // loop type
                0xF7              // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)1, sampleDumpHeaderSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual(3, sampleDumpHeaderSysExData.SampleNumber, "Sample number is invalid.");
            Assert.AreEqual(16, sampleDumpHeaderSysExData.SampleFormat, "Sample format is invalid.");
            Assert.AreEqual(22676, sampleDumpHeaderSysExData.SamplePeriod, "Sample period is invalid.");
            Assert.AreEqual(328064, sampleDumpHeaderSysExData.SampleLength, "Sample length is invalid.");
            Assert.AreEqual(0, sampleDumpHeaderSysExData.LoopStartPoint, "Loop start point is invalid.");
            Assert.AreEqual(328064, sampleDumpHeaderSysExData.LoopEndPoint, "Loop end point is invalid.");
            Assert.AreEqual(LoopType.Forward, sampleDumpHeaderSysExData.LoopType, "Loop type is invalid.");
        }

        [Test]
        public void ReadSysExData_SampleDumpDataPacket()
        {
            var sampleData = new byte[]
            {
                0x40, 0x15, 0x60, 0x40, 0x1B, 0x00, 0x40, 0x56, 0x40, 0x40,
                0x7D, 0x20, 0x40, 0x6D, 0x00, 0x41, 0x2D, 0x40, 0x41, 0x2D,
                0x60, 0x40, 0x47, 0x60, 0x40, 0x55, 0x60, 0x41, 0x15, 0x60,
                0x42, 0x1B, 0x60, 0x42, 0x56, 0x00, 0x43, 0x36, 0x00, 0x43,
                0x08, 0x60, 0x43, 0x4F, 0x20, 0x44, 0x24, 0x00, 0x44, 0x40,
                0x20, 0x45, 0x1C, 0x40, 0x44, 0x6D, 0x00, 0x44, 0x25, 0x60,
                0x44, 0x5C, 0x60, 0x44, 0x61, 0x40, 0x45, 0x14, 0x20, 0x45,
                0x36, 0x60, 0x45, 0x08, 0x60, 0x45, 0x0B, 0x40, 0x45, 0x2B,
                0x60, 0x45, 0x15, 0x00, 0x45, 0x11, 0x40, 0x45, 0x15, 0x20,
                0x45, 0x4D, 0x00, 0x45, 0x75, 0x60, 0x45, 0x7E, 0x60, 0x45,
                0x7D, 0x20, 0x46, 0x1C, 0x40, 0x46, 0x5A, 0x00, 0x46, 0x5A,
                0x40, 0x46, 0x46, 0x60, 0x46, 0x4D, 0x60, 0x46, 0x78, 0x60
            };

            var sampleDumpDataPacketSysExData = ReadSysExData<SampleDumpDataPacketSysExData>(new byte[]
            {
                0x7E,           // non-real time universal sys ex event
                0x02,           // channel
                0x02,           // 'sample dump data packet'
                0x05,           // packet number
            }
            .Concat(sampleData) // sample data
            .Concat(new byte[]
            {
                0x16,           // checksum
                0xF7            // end of sys ex event
            })
            .ToArray());

            Assert.AreEqual((SevenBitNumber)2, sampleDumpDataPacketSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual((SevenBitNumber)5, sampleDumpDataPacketSysExData.PacketNumber, "Packet number is invalid.");
            CollectionAssert.AreEqual(sampleData.Select(d => (SevenBitNumber)d), sampleDumpDataPacketSysExData.SampleData, "Sample data is invalid.");
            Assert.AreEqual((SevenBitNumber)22, sampleDumpDataPacketSysExData.Checksum, "Checksum is invalid.");
            Assert.IsTrue(sampleDumpDataPacketSysExData.IsValid, "Data is treated as invalid.");
        }

        [Test]
        public void ReadSysExData_SampleDumpRequest()
        {
            var sampleDumpRequestSysExData = ReadSysExData<SampleDumpRequestSysExData>(new byte[]
            {
                0x7E,       // non-real time universal sys ex event
                0x02,       // channel
                0x03,       // 'sample dump request'
                0x05, 0x1F, // sample number
                0xF7        // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)2, sampleDumpRequestSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual(3973, sampleDumpRequestSysExData.SampleNumber, "Sample number is invalid.");
        }

        [Test]
        public void ReadSysExData_Ack()
        {
            var ackSysExData = ReadSysExData<AckSysExData>(new byte[]
            {
                0x7E, // non-real time universal sys ex event
                0x02, // channel
                0x7F, // 'ACK'
                0x05, // packet number
                0xF7  // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)2, ackSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual((SevenBitNumber)5, ackSysExData.PacketNumber, "Packet number is invalid.");
        }

        [Test]
        public void ReadSysExData_Nak()
        {
            var nakSysExData = ReadSysExData<NakSysExData>(new byte[]
            {
                0x7E, // non-real time universal sys ex event
                0x00, // channel
                0x7E, // 'NAK'
                0x05, // packet number
                0xF7  // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)0, nakSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual((SevenBitNumber)5, nakSysExData.PacketNumber, "Packet number is invalid.");
        }

        [Test]
        public void ReadSysExData_Cancel()
        {
            var sampleDumpCancelSysExData = ReadSysExData<CancelSysExData>(new byte[]
            {
                0x7E, // non-real time universal sys ex event
                0x00, // channel
                0x7D, // 'CANCEL'
                0x00, // packet number
                0xF7  // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)0, sampleDumpCancelSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual((SevenBitNumber)0, sampleDumpCancelSysExData.PacketNumber, "Packet number is invalid.");
        }

        [Test]
        public void ReadSysExData_Wait()
        {
            var sampleDumpWaitSysExData = ReadSysExData<WaitSysExData>(new byte[]
            {
                0x7E, // non-real time universal sys ex event
                0x00, // channel
                0x7C, // 'WAIT'
                0x00, // packet number
                0xF7  // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)0, sampleDumpWaitSysExData.Channel, "Channel is invalid.");
            Assert.AreEqual((SevenBitNumber)0, sampleDumpWaitSysExData.PacketNumber, "Packet number is invalid.");
        }

        #endregion

        #region Private methods

        private static TData ReadSysExData<TData>(byte[] sysExDataBytes) where TData : SysExData
        {
            var sysExEvent = new NormalSysExEvent(sysExDataBytes);
            var sysExData = sysExEvent.ReadSysExData();
            Assert.IsInstanceOf<TData>(sysExData, $"SysEx data read is not of type {typeof(TData)}.");

            return (TData)sysExData;
        }

        #endregion
    }
}

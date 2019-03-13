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
                0x01,             // device ID
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

            Assert.AreEqual((SevenBitNumber)1, sampleDumpHeaderSysExData.DeviceId, "Device ID is invalid.");
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
                0x02,           // device ID
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

            Assert.AreEqual((SevenBitNumber)2, sampleDumpDataPacketSysExData.DeviceId, "Device ID is invalid.");
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
                0x02,       // device ID
                0x03,       // 'sample dump request'
                0x05, 0x1F, // sample number
                0xF7        // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)2, sampleDumpRequestSysExData.DeviceId, "Device ID is invalid.");
            Assert.AreEqual(3973, sampleDumpRequestSysExData.SampleNumber, "Sample number is invalid.");
        }

        [Test]
        public void ReadSysExData_Ack()
        {
            CheckReadingHandshakingSysExData<AckSysExData>(0x7F);
        }

        [Test]
        public void ReadSysExData_Nak()
        {
            CheckReadingHandshakingSysExData<NakSysExData>(0x7E);
        }

        [Test]
        public void ReadSysExData_Cancel()
        {
            CheckReadingHandshakingSysExData<CancelSysExData>(0x7D);
        }

        [Test]
        public void ReadSysExData_Wait()
        {
            CheckReadingHandshakingSysExData<WaitSysExData>(0x7C);
        }

        [Test]
        public void ReadSysExData_Eof()
        {
            CheckReadingHandshakingSysExData<EofSysExData>(0x7B);
        }

        [Test]
        public void ReadSysExData_MtcSpecial()
        {
            CheckReadingMtcSysExData<MtcSpecialSysExData>(0x00);
        }

        [Test]
        public void ReadSysExData_MtcPunchInPoint()
        {
            CheckReadingMtcSysExData<MtcPunchInSysExData>(0x01);
        }

        [Test]
        public void ReadSysExData_MtcPunchOutPoint()
        {
            CheckReadingMtcSysExData<MtcPunchOutSysExData>(0x02);
        }

        [Test]
        public void ReadSysExData_MtcDeletePunchInPoint()
        {
            CheckReadingMtcSysExData<MtcDeletePunchInSysExData>(0x03);
        }

        [Test]
        public void ReadSysExData_MtcDeletePunchOutPoint()
        {
            CheckReadingMtcSysExData<MtcDeletePunchOutSysExData>(0x04);
        }

        [Test]
        public void ReadSysExData_MtcEventStart()
        {
            CheckReadingMtcSysExData<MtcEventStartSysExData>(0x05);
        }

        [Test]
        public void ReadSysExData_MtcEventStop()
        {
            CheckReadingMtcSysExData<MtcEventStopSysExData>(0x06);
        }

        [Test]
        public void ReadSysExData_MtcEventStartWithInfo()
        {
            CheckReadingMtcWithInfoSysExData<MtcEventStartWithInfoSysExData>(0x07);
        }

        [Test]
        public void ReadSysExData_MtcEventStopWithInfo()
        {
            CheckReadingMtcWithInfoSysExData<MtcEventStopWithInfoSysExData>(0x08);
        }

        [Test]
        public void ReadSysExData_MtcDeleteEventStart()
        {
            CheckReadingMtcSysExData<MtcDeleteEventStartSysExData>(0x09);
        }

        [Test]
        public void ReadSysExData_MtcDeleteEventStop()
        {
            CheckReadingMtcSysExData<MtcDeleteEventStopSysExData>(0x0A);
        }

        [Test]
        public void ReadSysExData_MtcCuePoint()
        {
            CheckReadingMtcSysExData<MtcCuePointSysExData>(0x0B);
        }

        [Test]
        public void ReadSysExData_MtcCuePointWithInfo()
        {
            CheckReadingMtcWithInfoSysExData<MtcCuePointWithInfoSysExData>(0x0C);
        }

        [Test]
        public void ReadSysExData_MtcDeleteCuePoint()
        {
            CheckReadingMtcSysExData<MtcDeleteCuePointSysExData>(0x0D);
        }

        [Test]
        public void ReadSysExData_MtcEventNameInInfo()
        {
            CheckReadingMtcWithInfoSysExData<MtcEventNameInInfoSysExData>(0x0E);
        }

        #endregion

        #region Private methods

        private static TData CheckReadingMtcWithInfoSysExData<TData>(byte subId2) where TData : MtcWithInfoSysExData
        {
            var info = new byte[] { 1, 2, 3, 4 };
            var sysExData = CheckReadingMtcSysExData<TData>(subId2, info);

            CollectionAssert.AreEqual(info, sysExData.Info, "Info is invalid.");

            return sysExData;
        }

        private static TData CheckReadingMtcSysExData<TData>(byte subId2, params byte[] info) where TData : MtcSysExData
        {
            var sysExData = ReadSysExData<TData>(new byte[]
            {
                0x7E,      // non-real time universal sys ex event
                0x02,      // device ID
                0x04,      // MTC event 
                subId2,    //
                0x4C,      // 30 fps / 12 hours
                0x02,      // 2 minutes
                0x04,      // 4 seconds
                0x0A,      // 10 frames
                0x3F,      // 63 sub-frames
                0x03, 0x00 // event number
            }
            .Concat(info)
            .Concat(new byte[]
            {
                0xF7    // end of sys ex event
            })
            .ToArray());

            Assert.AreEqual((SevenBitNumber)2, sysExData.DeviceId, "Device ID is invalid.");
            Assert.AreEqual(SmpteFormat.ThirtyDrop, sysExData.Format, "Format is invalid.");
            Assert.AreEqual(12, sysExData.Hours, "Hours number is invalid.");
            Assert.AreEqual(2, sysExData.Minutes, "Minutes number is invalid.");
            Assert.AreEqual(4, sysExData.Seconds, "Seconds number is invalid.");
            Assert.AreEqual(10, sysExData.Frames, "Frames number is invalid.");
            Assert.AreEqual(63, sysExData.SubFrames, "Sub-frames number is invalid.");

            return sysExData;
        }

        private static void CheckReadingHandshakingSysExData<TData>(byte sysExDataId) where TData : HandshakingSysExData
        {
            var sysExData = ReadSysExData<TData>(new byte[]
            {
                0x7E,        // non-real time universal sys ex event
                0x00,        // device ID
                sysExDataId, // 
                0x00,        // packet number
                0xF7         // end of sys ex event
            });

            Assert.AreEqual((SevenBitNumber)0, sysExData.DeviceId, "Device ID is invalid.");
            Assert.AreEqual((SevenBitNumber)0, sysExData.PacketNumber, "Packet number is invalid.");
        }

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

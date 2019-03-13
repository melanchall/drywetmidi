using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SampleDumpHeaderSysExData : NonRealTimeSysExData
    {
        #region Constants

        private const byte MinSampleFormat = 8;
        private const byte MaxSampleFormat = 28;
        private const uint Max3ByteValue = 0xFFFFFF;

        #endregion

        #region Fields

        private byte _sampleFormat = MinSampleFormat;
        private uint _samplePeriod;
        private uint _sampleLength;
        private uint _loopStartPoint;
        private uint _loopEndPoint;

        #endregion

        #region Properties

        public ushort SampleNumber { get; set; }
        
        public byte SampleFormat
        {
            get { return _sampleFormat; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value), value, MinSampleFormat, MaxSampleFormat, "Sample format is out of range.");

                _sampleFormat = value;
            }
        }

        public uint SamplePeriod
        {
            get { return _samplePeriod; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, Max3ByteValue, "Sample period is out of range.");

                _samplePeriod = value;
            }
        }

        public uint SampleLength
        {
            get { return _sampleLength; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, Max3ByteValue, "Sample length is out of range.");

                _sampleLength = value;
            }
        }

        public uint LoopStartPoint
        {
            get { return _loopStartPoint; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, Max3ByteValue, "Loop start point is out of range.");

                _loopStartPoint = value;
            }
        }

        public uint LoopEndPoint
        {
            get { return _loopEndPoint; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, Max3ByteValue, "Loop end point is out of range.");

                _loopEndPoint = value;
            }
        }

        public LoopType LoopType { get; set; }

        #endregion

        #region Methods

        private static uint Read3SevenBitNumbersValue(MidiReader reader, SysExDataReadingSettings settings, string parameterName, bool lsbFirst)
        {
            var bytes = reader.ReadBytes(3);
            if (bytes.Length < 3)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read a 3-byte number.", 3, bytes.Length);

            var sevenBitNumbers = ToSevenBitNumbers(bytes, settings, parameterName);
            return lsbFirst
                ? DataTypesUtilities.Combine(sevenBitNumbers[2], sevenBitNumbers[1], sevenBitNumbers[0])
                : DataTypesUtilities.Combine(sevenBitNumbers[0], sevenBitNumbers[1], sevenBitNumbers[2]);
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            var sampleNumberLsb = ToSevenBitNumber(reader.ReadByte(), settings, "sample number LSB");
            var sampleNumberMsb = ToSevenBitNumber(reader.ReadByte(), settings, "sample number MSB");
            SampleNumber = DataTypesUtilities.Combine(sampleNumberMsb, sampleNumberLsb);

            var sampleFormat = reader.ReadByte();
            if (sampleFormat < MinSampleFormat || sampleFormat > MaxSampleFormat)
                throw new InvalidSysExDataParameterException("Sample format is invalid.", sampleFormat);

            SampleFormat = sampleFormat;

            SamplePeriod = Read3SevenBitNumbersValue(reader, settings, "sample period", lsbFirst: true);
            SampleLength = Read3SevenBitNumbersValue(reader, settings, "sample length", lsbFirst: false);

            LoopStartPoint = Read3SevenBitNumbersValue(reader, settings, "loop start point", lsbFirst: true);
            LoopEndPoint = Read3SevenBitNumbersValue(reader, settings, "loop end point", lsbFirst: false);

            var loopType = reader.ReadByte();
            if (!Enum.IsDefined(typeof(LoopType), (int)loopType))
                throw new InvalidSysExDataParameterException("Loop type is invalid.", loopType);

            LoopType = (LoopType)loopType;
        }

        public override string ToString()
        {
            return "Sample Dump Header";
        }

        #endregion
    }
}

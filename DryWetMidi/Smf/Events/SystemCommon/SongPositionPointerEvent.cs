using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SongPositionPointerEvent : SystemCommonEvent
    {
        #region Constructor

        public SongPositionPointerEvent()
        {
        }

        public SongPositionPointerEvent(SevenBitNumber msb, SevenBitNumber lsb)
        {
            Msb = msb;
            Lsb = lsb;
        }

        #endregion

        #region Properties

        public SevenBitNumber Msb { get; set; }

        public SevenBitNumber Lsb { get; set; }

        #endregion

        #region Methods

        private static SevenBitNumber ProcessValue(byte value, string property, InvalidSystemCommonEventParameterValuePolicy policy)
        {
            if (value > SevenBitNumber.MaxValue)
            {
                switch (policy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException($"{value} is invalid value for the {property} of a Song Position Pointer event.", value);
                    case InvalidSystemCommonEventParameterValuePolicy.SnapToLimits:
                        return SevenBitNumber.MaxValue;
                }
            }

            return (SevenBitNumber)value;
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            Lsb = ProcessValue(reader.ReadByte(), nameof(Lsb), settings.InvalidSystemCommonEventParameterValuePolicy);
            Msb = ProcessValue(reader.ReadByte(), nameof(Msb), settings.InvalidSystemCommonEventParameterValuePolicy);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Lsb);
            writer.WriteByte(Msb);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 2;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SongPositionPointerEvent(Msb, Lsb);
        }

        public override string ToString()
        {
            return $"Song Position Pointer ({Msb}, {Lsb})";
        }

        #endregion
    }
}

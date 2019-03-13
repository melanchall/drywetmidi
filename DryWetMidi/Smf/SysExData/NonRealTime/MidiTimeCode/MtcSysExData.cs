using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public abstract class MtcSysExData : NonRealTimeSysExData
    {
        #region Fields

        private SmpteData _smpteData = new SmpteData();

        #endregion

        #region Properties

        public SmpteFormat Format
        {
            get { return _smpteData.Format; }
            set { _smpteData.Format = value; }
        }

        public byte Hours
        {
            get { return _smpteData.Hours; }
            set { _smpteData.Hours = value; }
        }

        public byte Minutes
        {
            get { return _smpteData.Minutes; }
            set { _smpteData.Minutes = value; }
        }

        public byte Seconds
        {
            get { return _smpteData.Seconds; }
            set { _smpteData.Seconds = value; }
        }

        public byte Frames
        {
            get { return _smpteData.Frames; }
            set { _smpteData.Frames = value; }
        }

        public byte SubFrames
        {
            get { return _smpteData.SubFrames; }
            set { _smpteData.SubFrames = value; }
        }

        public ushort EventNumber { get; set; }

        #endregion

        #region Methods

        private static byte ProcessValue(byte value, string property, byte max, InvalidSysExDataParameterValuePolicy policy)
        {
            if (value <= max)
                return value;

            switch (policy)
            {
                case InvalidSysExDataParameterValuePolicy.Abort:
                    throw new InvalidSystemCommonEventParameterValueException($"{value} is invalid value for the {property} of a MTC data.", value);
                case InvalidSysExDataParameterValuePolicy.SnapToLimits:
                    return Math.Min(value, max);
            }

            return value;
        }
        
        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            _smpteData = SmpteData.Read(
                reader.ReadByte,
                (value, propertyName, max) => ProcessValue(value, propertyName, max, settings.InvalidSysExDataParameterValuePolicy));

            var eventNumberLsb = reader.ReadByte();
            var eventNumberMsb = reader.ReadByte();
            EventNumber = DataTypesUtilities.Combine(eventNumberMsb, eventNumberLsb);
        }

        #endregion
    }
}

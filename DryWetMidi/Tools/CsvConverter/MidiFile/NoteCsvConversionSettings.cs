using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NoteCsvConversionSettings
    {
        #region Fields

        private TimeSpanType _lengthType = TimeSpanType.Midi;
        private NoteFormat _noteFormat = NoteFormat.Events;
        private NoteNumberFormat _noteNumberFormat = NoteNumberFormat.NoteNumber;

        #endregion

        #region Properties

        public TimeSpanType LengthType
        {
            get { return _lengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _lengthType = value;
            }
        }

        public NoteFormat NoteFormat
        {
            get { return _noteFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteFormat = value;
            }
        }

        public NoteNumberFormat NoteNumberFormat
        {
            get { return _noteNumberFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteNumberFormat = value;
            }
        }

        #endregion
    }
}

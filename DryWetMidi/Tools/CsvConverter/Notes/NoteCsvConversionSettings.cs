using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NoteCsvConversionSettings
    {
        #region Fields

        private TimeSpanType _timeType = TimeSpanType.Midi;
        private TimeSpanType _noteLengthType = TimeSpanType.Midi;
        private NoteNumberFormat _noteNumberFormat = NoteNumberFormat.NoteNumber;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets delimiter used to separate values in CSV representation. The default value is comma.
        /// </summary>
        public char CsvDelimiter { get; set; } = ',';

        public TimeSpanType TimeType
        {
            get { return _timeType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _timeType = value;
            }
        }

        public TimeSpanType NoteLengthType
        {
            get { return _noteLengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteLengthType = value;
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

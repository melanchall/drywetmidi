using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class CsvSerializationSettings
    {
        #region Fields

        private TimeSpanType _timeType = TimeSpanType.Midi;
        private TimeSpanType _lengthType = TimeSpanType.Midi;
        private CsvNoteFormat _noteFormat = CsvNoteFormat.NoteNumber;
        private CsvBytesArrayFormat _bytesArrayFormat = CsvBytesArrayFormat.Decimal;

        private int _readWriteBufferSize = 1024;

        #endregion

        #region Properties

        public TimeSpanType TimeType
        {
            get { return _timeType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _timeType = value;
            }
        }

        public TimeSpanType LengthType
        {
            get { return _lengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _lengthType = value;
            }
        }

        public CsvNoteFormat NoteNumberFormat
        {
            get { return _noteFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteFormat = value;
            }
        }

        public CsvBytesArrayFormat BytesArrayFormat
        {
            get { return _bytesArrayFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _bytesArrayFormat = value;
            }
        }

        public char Delimiter { get; set; } = ',';

        public int ReadWriteBufferSize
        {
            get { return _readWriteBufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Buffer size is zero or negative.");

                _readWriteBufferSize = value;
            }
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System.ComponentModel;
using System;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class CsvDeserializationSettings
    {
        #region Fields

        private TimeSpanType? _timeType = null;
        private TimeSpanType? _lengthType = null;
        private CsvNoteFormat? _noteFormat = null;
        private CsvBytesArrayFormat _bytesArrayFormat = CsvBytesArrayFormat.Decimal;
        private UnknownRecordPolicy _unknownRecordPolicy = UnknownRecordPolicy.Abort;

        private int _bufferSize = 1024;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value that defines how the time of objects is presented in CSV.
        /// The default value is <c>null</c> which means time format will be resolved automatically.
        /// More info on the supported formats in the <see href="xref:a_time_length">Time and length</see> article.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType? TimeType
        {
            get { return _timeType; }
            set
            {
                if (value != null)
                    ThrowIfArgument.IsInvalidEnumValue(nameof(value), value.Value);

                _timeType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how length of notes is presented in CSV.
        /// The default value is <c>null</c> which means length format will be resolved automatically.
        /// More info on the supported formats in the <see href="xref:a_time_length">Time and length</see> article.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType? LengthType
        {
            get { return _lengthType; }
            set
            {
                if (value != null)
                    ThrowIfArgument.IsInvalidEnumValue(nameof(value), value.Value);

                _lengthType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how note numbers (for example, <see cref="NoteAftertouchEvent.NoteNumber"/>)
        /// are presented in CSV. The default value is <c>null</c> which means note format will be resolved automatically.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public CsvNoteFormat? NoteFormat
        {
            get { return _noteFormat; }
            set
            {
                if (value != null)
                    ThrowIfArgument.IsInvalidEnumValue(nameof(value), value.Value);

                _noteFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how bytes arrays (for example, <see cref="SequencerSpecificEvent.Data"/>)
        /// are presented in CSV. The default value is <see cref="CsvBytesArrayFormat.Decimal"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public CsvBytesArrayFormat BytesArrayFormat
        {
            get { return _bytesArrayFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _bytesArrayFormat = value;
            }
        }

        public UnknownRecordPolicy UnknownRecordPolicy
        {
            get { return _unknownRecordPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _unknownRecordPolicy = value;
            }
        }

        public char BytesArrayDelimiter { get; set; } = ' ';

        /// <summary>
        /// Gets or sets a char used as the values delimiter in CSV records. The default value
        /// is comma (<c>','</c>).
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the size in bytes of the internal buffer for reading data within
        /// the process of CSV deserialization. The default value is <c>1024</c>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Buffer size is zero or negative.");

                _bufferSize = value;
            }
        }

        #endregion
    }
}

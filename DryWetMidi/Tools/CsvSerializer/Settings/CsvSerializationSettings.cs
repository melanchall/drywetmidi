using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Privides settings that control the process of CSV serialization/deserialization.
    /// </summary>
    /// <seealso cref="CsvSerializer"/>
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

        /// <summary>
        /// Gets or sets a value that defines how the time of objects should be presented in CSV.
        /// The default value is <see cref="TimeSpanType.Midi"/>. More info on the supported
        /// formats in the <see href="xref:a_time_length">Time and length</see> article.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType TimeType
        {
            get { return _timeType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _timeType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how length of notes should be presented in CSV.
        /// The default value is <see cref="TimeSpanType.Midi"/>. More info on the supported
        /// formats in the <see href="xref:a_time_length">Time and length</see> article.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType LengthType
        {
            get { return _lengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _lengthType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how note numbers (for example, <see cref="NoteAftertouchEvent.NoteNumber"/>)
        /// should be presented in CSV. The default value is <see cref="CsvNoteFormat.NoteNumber"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public CsvNoteFormat NoteFormat
        {
            get { return _noteFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines how bytes arrays (for example, <see cref="SequencerSpecificEvent.Data"/>)
        /// should be presented in CSV. The default value is <see cref="CsvBytesArrayFormat.Decimal"/>.
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

        /// <summary>
        /// Gets or sets a char used as the values delimiter in CSV records. The default value
        /// is comma (<c>','</c>).
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the size in bytes of the internal buffer for reading/writing data within
        /// the process of CSV serialization/deserialization. The default value is <c>1024</c>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
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

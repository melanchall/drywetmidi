using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which <see cref="MidiFile"/> must be read from or written to
    /// CSV representation.
    /// </summary>
    public sealed class MidiFileCsvConversionSettings
    {
        #region Fields

        private MidiFileCsvLayout _csvLayout = MidiFileCsvLayout.DryWetMidi;
        private TimeSpanType _timeType = TimeSpanType.Midi;
        private TimeSpanType _noteLengthType = TimeSpanType.Midi;
        private NoteFormat _noteFormat = NoteFormat.Events;
        private NoteNumberFormat _noteNumberFormat = NoteNumberFormat.NoteNumber;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets layout of CSV representation of <see cref="MidiFile"/>. The default value is
        /// <see cref="MidiFileCsvLayout.DryWetMidi"/>.
        /// </summary>
        /// <remarks>
        /// At now there are two layouts: <see cref="MidiFileCsvLayout.DryWetMidi"/> and
        /// <see cref="MidiFileCsvLayout.MidiCsv"/> which produces slightly different CSV representations.
        /// The default value is <see cref="MidiFileCsvLayout.DryWetMidi"/> that gives more compact and more
        /// human readable CSV data.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public MidiFileCsvLayout CsvLayout
        {
            get { return _csvLayout; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _csvLayout = value;
            }
        }

        /// <summary>
        /// Gets or sets format of timestamps inside CSV representation. The default value is <see cref="TimeSpanType.Midi"/>
        /// </summary>
        /// <remarks>
        /// Note that it is recommended to use <see cref="TimeSpanType.Midi"/> if you use
        /// <see cref="MidiFileCsvLayout.MidiCsv"/> CSV layout to ensure produced CSV data can be read
        /// by other readers that supports format used by midicsv (http://www.fourmilab.ch/webtools/midicsv/) program.
        /// </remarks>
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
        /// Gets or sets the type of a note length (metric, bar/beat and so on) which should be used to
        /// write to or read from CSV. The default value is <see cref="TimeSpanType.Midi"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType NoteLengthType
        {
            get { return _noteLengthType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteLengthType = value;
            }
        }

        /// <summary>
        /// Gets or sets the format which should be used to write notes to or read them from CSV.
        /// The default value is <see cref="Tools.NoteFormat.Events"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NoteFormat NoteFormat
        {
            get { return _noteFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the format which should be used to write a note's number to or read it from CSV.
        /// The default value is <see cref="Tools.NoteNumberFormat.NoteNumber"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NoteNumberFormat NoteNumberFormat
        {
            get { return _noteNumberFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteNumberFormat = value;
            }
        }

        /// <summary>
        /// Gets common CSV settings.
        /// </summary>
        public CsvSettings CsvSettings { get; } = new CsvSettings();

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a tempo map of a MIDI file.
    /// </summary>
    public sealed class TempoMap
    {
        #region Constants

        public static readonly TempoMap Default = new TempoMap(new TicksPerQuarterNoteTimeDivision());

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMap"/> with the specified time division
        /// of a MIDI file.
        /// </summary>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is null.</exception>
        internal TempoMap(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            TimeDivision = timeDivision;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time division used by a tempo map.
        /// </summary>
        public TimeDivision TimeDivision { get; internal set; }

        /// <summary>
        /// Gets an object that holds changes of the time signature through the time.
        /// </summary>
        public ValueLine<TimeSignature> TimeSignatureLine { get; private set; } = new ValueLine<TimeSignature>(TimeSignature.Default);

        /// <summary>
        /// Gets an object that holds changes of the tempo through the time.
        /// </summary>
        public ValueLine<Tempo> TempoLine { get; private set; } = new ValueLine<Tempo>(Tempo.Default);

        #endregion

        #region Methods

        internal TempoMap Reverse(long centerTime)
        {
            return new TempoMap(TimeDivision)
            {
                TempoLine = TempoLine.Reverse(centerTime),
                TimeSignatureLine = TimeSignatureLine.Reverse(centerTime)
            };
        }

        #endregion
    }
}

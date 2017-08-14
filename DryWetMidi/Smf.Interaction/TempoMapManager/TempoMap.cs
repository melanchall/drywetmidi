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

        /// <summary>
        /// The default tempo map which uses 4/4 time signature and tempo of 500,000 microseconds per quarter note.
        /// </summary>
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
        public ValueLine<TimeSignature> TimeSignature { get; private set; } = new ValueLine<TimeSignature>(Interaction.TimeSignature.Default);

        /// <summary>
        /// Gets an object that holds changes of the tempo through the time.
        /// </summary>
        public ValueLine<Tempo> Tempo { get; private set; } = new ValueLine<Tempo>(Interaction.Tempo.Default);

        #endregion

        #region Methods

        /// <summary>
        /// Clones the current <see cref="TempoMap"/>.
        /// </summary>
        /// <returns>An instance of the <see cref="TempoMap"/> which is a clone of the current one.</returns>
        public TempoMap Clone()
        {
            var tempoMap = new TempoMap(TimeDivision.Clone());

            tempoMap.Tempo.ReplaceValues(Tempo);
            tempoMap.TimeSignature.ReplaceValues(TimeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo and
        /// time signature using default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo and time signature.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is null. -or-
        /// <paramref name="timeSignature"/> is null.</exception>
        public static TempoMap Create(Tempo tempo, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = Default.Clone();
            tempoMap.Tempo.SetValue(0, tempo);
            tempoMap.TimeSignature.SetValue(0, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo using
        /// default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is null.</exception>
        public static TempoMap Create(Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            var tempoMap = Default.Clone();
            tempoMap.Tempo.SetValue(0, tempo);

            return tempoMap;
        }

        /// <summary>
        /// Flips the tempo map relative to the specified time.
        /// </summary>
        /// <param name="centerTime">The time the tempo map should be flipped relative to.</param>
        /// <returns>The tempo mup flipped relative to the <paramref name="centerTime"/>.</returns>
        internal TempoMap Flip(long centerTime)
        {
            return new TempoMap(TimeDivision)
            {
                Tempo = Tempo.Reverse(centerTime),
                TimeSignature = TimeSignature.Reverse(centerTime)
            };
        }

        #endregion
    }
}

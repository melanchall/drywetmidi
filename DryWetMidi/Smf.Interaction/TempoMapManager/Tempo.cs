using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents tempo expressed in microseconds per quarter note or beats per minute.
    /// </summary>
    public sealed class Tempo
    {
        #region Constants

        /// <summary>
        /// Default tempo which is 500,000 microseconds per quarter note or 120 beats per minute.
        /// </summary>
        public static readonly Tempo Default = new Tempo(SetTempoEvent.DefaultTempo);

        #endregion

        #region Fields

        private const int MicrosecondsInMinute = 60000000;
        private const int MicrosecondsInMillisecond = 1000;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Tempo"/> with the specified number of
        /// microseconds per quarter note.
        /// </summary>
        /// <param name="microsecondsPerQuarterNote">Number of microseconds per quarter note.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="microsecondsPerQuarterNote"/>
        /// is zero or negative.</exception>
        public Tempo(long microsecondsPerQuarterNote)
        {
            if (microsecondsPerQuarterNote <= 0)
                throw new ArgumentOutOfRangeException("Number of microseconds per quarter note is zero or negative.",
                                                      microsecondsPerQuarterNote,
                                                      nameof(microsecondsPerQuarterNote));

            MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of microseconds per quarter note.
        /// </summary>
        public long MicrosecondsPerQuarterNote { get; }

        /// <summary>
        /// Gets number of beats per minute.
        /// </summary>
        public long BeatsPerMinute => MicrosecondsInMinute / MicrosecondsPerQuarterNote;

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="Tempo"/> with the specified number of
        /// milliseconds per quarter note.
        /// </summary>
        /// <param name="millisecondsPerQuarterNote">Number of milliseconds per quarter note.</param>
        /// <returns>An instance of the <see cref="Tempo"/> which represents tempo as specified
        /// number of milliseconds per quarter note.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsPerQuarterNote"/>
        /// is zero or negative.</exception>
        public static Tempo FromMillisecondsPerQuarterNote(long millisecondsPerQuarterNote)
        {
            if (millisecondsPerQuarterNote <= 0)
                throw new ArgumentOutOfRangeException("Number of milliseconds per quarter note is zero or negative.",
                                                      millisecondsPerQuarterNote,
                                                      nameof(millisecondsPerQuarterNote));

            return new Tempo(millisecondsPerQuarterNote * MicrosecondsInMillisecond);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Tempo"/> with the specified number of
        /// beats per minute.
        /// </summary>
        /// <param name="beatsPerMinute">Number of beats per minute.</param>
        /// <returns>An instance of the <see cref="Tempo"/> which represents tempo as specified
        /// number of beats per minute.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="beatsPerMinute"/>
        /// is zero or negative.</exception>
        public static Tempo FromBeatsPerMinute(long beatsPerMinute)
        {
            if (beatsPerMinute <= 0)
                throw new ArgumentOutOfRangeException("Number of beats per minute is zero or negative.",
                                                      beatsPerMinute,
                                                      nameof(beatsPerMinute));

            return new Tempo(MicrosecondsInMinute / beatsPerMinute);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{MicrosecondsPerQuarterNote} μs/qnote";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var tempo = obj as Tempo;
            if (ReferenceEquals(null, tempo))
                return false;

            return MicrosecondsPerQuarterNote == tempo.MicrosecondsPerQuarterNote;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return MicrosecondsPerQuarterNote.GetHashCode();
        }

        #endregion
    }
}

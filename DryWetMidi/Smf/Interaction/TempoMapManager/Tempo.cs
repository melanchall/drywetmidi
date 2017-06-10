using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Tempo
    {
        #region Constants

        public static readonly Tempo Default = new Tempo(SetTempoEvent.DefaultTempo);

        #endregion

        #region Fields

        private const int MicrosecondsInMinute = 60000000;
        private const int MicrosecondsInMillisecond = 1000;

        #endregion

        #region Constructor

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

        public long MicrosecondsPerQuarterNote { get; }

        public long BeatsPerMinute => MicrosecondsInMinute / MicrosecondsPerQuarterNote;

        #endregion

        #region Methods

        public static Tempo FromMillisecondsPerQuarterNote(long millisecondsPerQuarterNote)
        {
            if (millisecondsPerQuarterNote <= 0)
                throw new ArgumentOutOfRangeException("Number of milliseconds per quarter note is zero or negative.",
                                                      millisecondsPerQuarterNote,
                                                      nameof(millisecondsPerQuarterNote));

            return new Tempo(millisecondsPerQuarterNote * MicrosecondsInMillisecond);
        }

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

        public override string ToString()
        {
            return $"{MicrosecondsPerQuarterNote} μs/qnote";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var tempo = obj as Tempo;
            if (ReferenceEquals(null, tempo))
                return false;

            return MicrosecondsPerQuarterNote == tempo.MicrosecondsPerQuarterNote;
        }

        public override int GetHashCode()
        {
            return MicrosecondsPerQuarterNote.GetHashCode();
        }

        #endregion
    }
}

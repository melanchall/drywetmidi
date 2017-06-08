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

        public Tempo(long microsecondsPerBeat)
        {
            if (microsecondsPerBeat <= 0)
                throw new ArgumentOutOfRangeException("Number of microseconds per beat is zero or negative.",
                                                      microsecondsPerBeat,
                                                      nameof(microsecondsPerBeat));

            MicrosecondsPerBeat = microsecondsPerBeat;
        }

        #endregion

        #region Properties

        public long MicrosecondsPerBeat { get; }

        public long BeatsPerMinute => MicrosecondsInMinute / MicrosecondsPerBeat;

        #endregion

        #region Methods

        public static Tempo FromMillisecondsPerBeat(long millisecondsPerBeat)
        {
            if (millisecondsPerBeat <= 0)
                throw new ArgumentOutOfRangeException("Number of milliseconds per beat is zero or negative.",
                                                      millisecondsPerBeat,
                                                      nameof(millisecondsPerBeat));

            return new Tempo(millisecondsPerBeat * MicrosecondsInMillisecond);
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
            return $"{MicrosecondsPerBeat} μs/beat";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var tempo = obj as Tempo;
            if (ReferenceEquals(null, tempo))
                return false;

            return MicrosecondsPerBeat == tempo.MicrosecondsPerBeat;
        }

        public override int GetHashCode()
        {
            return MicrosecondsPerBeat.GetHashCode();
        }

        #endregion
    }
}

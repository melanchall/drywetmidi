using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalTime : ITime
    {
        #region Constructor

        public MusicalTime(int bars, int beats, int ticks, int beatLength)
        {
            if (bars < 0)
                throw new ArgumentOutOfRangeException("Number of bars is negative.", bars, nameof(bars));

            if (beats < 0)
                throw new ArgumentOutOfRangeException("Number of beats is negative.", beats, nameof(beats));

            if (ticks < 0)
                throw new ArgumentOutOfRangeException("Number of ticks is negative.", ticks, nameof(ticks));

            if (beatLength < 0)
                throw new ArgumentOutOfRangeException("Beat length is negative.", beatLength, nameof(beatLength));

            Bars = bars;
            Beats = beats;
            Ticks = ticks;
            BeatLength = beatLength;
        }

        #endregion

        #region Properties

        public int Bars { get; }

        public int Beats { get; }

        public int Ticks { get; }

        public int BeatLength { get; }

        #endregion

        #region Methods

        public bool Equals(MusicalTime time)
        {
            if (ReferenceEquals(null, time))
                return false;

            if (ReferenceEquals(this, time))
                return true;

            return Bars == time.Bars &&
                   Beats == time.Beats &&
                   Ticks == time.Ticks &&
                   BeatLength == time.BeatLength;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MusicalTime);
        }

        public override int GetHashCode()
        {
            return Bars.GetHashCode() ^
                   Beats.GetHashCode() ^
                   Ticks.GetHashCode() ^
                   BeatLength.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Bars}:{Beats}[{BeatLength}]:{Ticks}";
        }

        #endregion
    }
}

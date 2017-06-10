using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalTime : ITime
    {
        #region Constructor

        public MusicalTime(int bars, int beats, int ticks, int beatLength)
        {
            if (bars < 0)
                throw new ArgumentOutOfRangeException("Bar number is negative.", bars, nameof(bars));

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

        #region Overrides

        public override string ToString()
        {
            return $"{Bars}:{Beats}[{BeatLength}]:{Ticks}";
        }

        #endregion
    }
}

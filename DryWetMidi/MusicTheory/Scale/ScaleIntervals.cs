using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        public static readonly Interval[] Lydian = GetIntervalsDefinitions(2, 2, 2, 1, 2, 2, 1);

        public static readonly Interval[] Ionian = GetIntervalsDefinitions(2, 2, 1, 2, 2, 2, 1);

        public static readonly Interval[] Major = Ionian;

        public static readonly Interval[] Mixolydian = GetIntervalsDefinitions(2, 2, 1, 2, 2, 1, 2);

        public static readonly Interval[] Aeolian = GetIntervalsDefinitions(2, 1, 2, 2, 2, 1, 2);

        public static readonly Interval[] Minor = Aeolian;

        public static readonly Interval[] Dorian = GetIntervalsDefinitions(2, 1, 2, 2, 1, 2, 2);

        public static readonly Interval[] Phrygian = GetIntervalsDefinitions(1, 2, 2, 2, 1, 2, 2);

        public static readonly Interval[] Lochrian = GetIntervalsDefinitions(1, 2, 2, 1, 2, 2, 2);

        #endregion

        #region Methods

        private static Interval[] GetIntervalsDefinitions(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => Interval.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}

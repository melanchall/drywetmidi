using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        public static readonly Interval[] Lydian = GetIntervals(2, 2, 2, 1, 2, 2, 1);

        public static readonly Interval[] Ionian = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        public static readonly Interval[] Major = Ionian;

        public static readonly Interval[] Mixolydian = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        public static readonly Interval[] Aeolian = GetIntervals(2, 1, 2, 2, 2, 1, 2);

        public static readonly Interval[] Minor = Aeolian;

        public static readonly Interval[] Dorian = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        public static readonly Interval[] Phrygian = GetIntervals(1, 2, 2, 2, 1, 2, 2);

        public static readonly Interval[] Lochrian = GetIntervals(1, 2, 2, 1, 2, 2, 2);

        #endregion

        #region Methods

        private static Interval[] GetIntervals(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => Interval.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}

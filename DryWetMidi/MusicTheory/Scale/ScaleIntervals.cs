using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        public static readonly IntervalDefinition[] Lydian = GetIntervalsDefinitions(2, 2, 2, 1, 2, 2, 1);

        public static readonly IntervalDefinition[] Ionian = GetIntervalsDefinitions(2, 2, 1, 2, 2, 2, 1);

        public static readonly IntervalDefinition[] Major = Ionian;

        public static readonly IntervalDefinition[] Mixolydian = GetIntervalsDefinitions(2, 2, 1, 2, 2, 1, 2);

        public static readonly IntervalDefinition[] Aeolian = GetIntervalsDefinitions(2, 1, 2, 2, 2, 1, 2);

        public static readonly IntervalDefinition[] Minor = Aeolian;

        public static readonly IntervalDefinition[] Dorian = GetIntervalsDefinitions(2, 1, 2, 2, 1, 2, 2);

        public static readonly IntervalDefinition[] Phrygian = GetIntervalsDefinitions(1, 2, 2, 2, 1, 2, 2);

        public static readonly IntervalDefinition[] Lochrian = GetIntervalsDefinitions(1, 2, 2, 1, 2, 2, 2);

        #endregion

        #region Methods

        private static IntervalDefinition[] GetIntervalsDefinitions(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => IntervalDefinition.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}

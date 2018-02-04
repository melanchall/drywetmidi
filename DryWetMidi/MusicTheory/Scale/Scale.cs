using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public sealed class Scale
    {
        #region Constructor

        public Scale(IEnumerable<IntervalDefinition> intervals, NoteName rootNoteName)
        {
            ThrowIfArgument.IsNull(nameof(intervals), intervals);
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);

            Intervals = intervals;
            RootNoteName = rootNoteName;
        }

        #endregion

        #region Properties

        public IEnumerable<IntervalDefinition> Intervals { get; }

        public NoteName RootNoteName { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{string.Join(" ", Intervals)} from {RootNoteName}";
        }

        #endregion
    }
}

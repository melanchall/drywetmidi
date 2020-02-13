using System;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiEventEqualityCheckSettings
    {
        #region Properties

        public bool CompareDeltaTimes { get; set; } = true;

        public StringComparison TextComparison { get; set; } = StringComparison.CurrentCulture;

        #endregion
    }
}

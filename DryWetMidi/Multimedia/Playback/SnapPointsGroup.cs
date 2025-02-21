using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class SnapPointsGroup
    {
        #region Constructor

        internal SnapPointsGroup(Predicate<MidiEvent> predicate)
        {
            Predicate = predicate;
        }

        #endregion

        #region Properties

        internal Predicate<MidiEvent> Predicate { get; }

        public bool IsEnabled { get; set; } = true;

        #endregion
    }
}

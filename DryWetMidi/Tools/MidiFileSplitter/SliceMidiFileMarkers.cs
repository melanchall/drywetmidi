using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SliceMidiFileMarkers
    {
        #region Properties

        public Func<MidiEvent> PartStartMarkerEventFactory { get; set; }

        public Func<MidiEvent> PartEndMarkerEventFactory { get; set; }

        public Func<MidiEvent> EmptyPartMarkerEventFactory { get; set; }

        #endregion
    }
}

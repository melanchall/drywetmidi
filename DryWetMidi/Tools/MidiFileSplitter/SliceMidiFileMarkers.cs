using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides factory methods to create events to mark parts of split file.
    /// </summary>
    public sealed class SliceMidiFileMarkers
    {
        #region Properties

        /// <summary>
        /// Gets or sets a factory method to create event that will be placed at the start
        /// of a split MIDI file part.
        /// </summary>
        public Func<MidiEvent> PartStartMarkerEventFactory { get; set; }

        /// <summary>
        /// Gets or sets a factory method to create event that will be placed at the end
        /// of a split MIDI file part.
        /// </summary>
        public Func<MidiEvent> PartEndMarkerEventFactory { get; set; }

        /// <summary>
        /// Gets or sets a factory method to create event that will be placed in a split MIDI
        /// file part if it's empty.
        /// </summary>
        public Func<MidiEvent> EmptyPartMarkerEventFactory { get; set; }

        #endregion
    }
}

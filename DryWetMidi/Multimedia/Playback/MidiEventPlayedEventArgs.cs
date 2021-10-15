using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds an instance of <see cref="MidiEvent"/> for <see cref="Playback.EventPlayed"/> event.
    /// </summary>
    public sealed class MidiEventPlayedEventArgs : EventArgs
    {
        #region Constructor

        internal MidiEventPlayedEventArgs(MidiEvent midiEvent, object metadata)
        {
            Event = midiEvent;
            Metadata = metadata;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a MIDI event played.
        /// </summary>
        public MidiEvent Event { get; }

        /// <summary>
        /// Gets a metadata associated with the <see cref="Event"/>.
        /// </summary>
        public object Metadata { get; }

        #endregion
    }
}

using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Holds an instance of <see cref="MidiEvent"/> for <see cref="Playback.EventPlayed"/> event.
    /// </summary>
    public sealed class MidiEventPlayedEventArgs : EventArgs
    {
        #region Constructor

        internal MidiEventPlayedEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a MIDI event played.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

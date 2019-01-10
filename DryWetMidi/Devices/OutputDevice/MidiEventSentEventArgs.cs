using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides data for the <see cref="OutputDevice.EventSent"/> event.
    /// </summary>
    public sealed class MidiEventSentEventArgs : EventArgs
    {
        #region Constructor

        internal MidiEventSentEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets MIDI event sent to <see cref="OutputDevice"/>.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

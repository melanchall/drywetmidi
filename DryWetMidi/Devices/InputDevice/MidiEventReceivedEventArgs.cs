using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides data for the <see cref="InputDevice.EventReceived"/> event.
    /// </summary>
    public sealed class MidiEventReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal MidiEventReceivedEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets MIDI event received by <see cref="InputDevice"/>.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides data for the <see cref="IOutputDevice.EventSent"/> event.
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
        /// Gets MIDI event sent to <see cref="IOutputDevice"/>.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

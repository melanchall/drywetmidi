using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Devices
{
    public interface IOutputDevice
    {
        /// <summary>
        /// Occurs when a MIDI event is sent.
        /// </summary>
        event EventHandler<MidiEventSentEventArgs> EventSent;

        void PrepareForEventsSending();

        void SendEvent(MidiEvent midiEvent);
    }
}

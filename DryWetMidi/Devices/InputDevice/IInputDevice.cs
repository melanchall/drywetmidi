using System;

namespace Melanchall.DryWetMidi.Devices
{
    public interface IInputDevice
    {
        /// <summary>
        /// Occurs when a MIDI event is received.
        /// </summary>
        event EventHandler<MidiEventReceivedEventArgs> EventReceived;

        bool IsListeningForEvents { get; }

        void StartEventsListening();

        void StopEventsListening();
    }
}

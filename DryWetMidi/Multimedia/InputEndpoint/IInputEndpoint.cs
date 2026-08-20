using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an abstract input MIDI endpoint. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_input">Input endpoint</see> articles.
    /// </summary>
    public interface IInputEndpoint : IDisposable
    {
        /// <summary>
        /// Occurs when a MIDI event is received.
        /// </summary>
        event EventHandler<MidiEventReceivedEventArgs>? EventReceived;

        /// <summary>
        /// Gets a value that indicates whether the current input endpoint is currently listening for
        /// incoming MIDI events.
        /// </summary>
        bool IsListeningForEvents { get; }

        /// <summary>
        /// Starts listening for incoming MIDI events on the current input endpoint.
        /// </summary>
        void StartEventsListening();

        /// <summary>
        /// Stops listening for incoming MIDI events on the current input endpoint.
        /// </summary>
        void StopEventsListening();
    }
}

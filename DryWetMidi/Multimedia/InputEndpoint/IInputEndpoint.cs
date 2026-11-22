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

        /// <summary>
        /// Gets the current system timestamp in nanoseconds.
        /// </summary>
        /// <returns>The current system timestamp in nanoseconds.</returns>
        /// <remarks>
        /// Use this timestamp as a baseline for timestamps on incoming MIDI events. Event timestamps use the same
        /// system monotonic clock, so they can be compared with this value and with timestamps from other input endpoints.
        /// </remarks>
        long GetCurrentTimestamp();
    }
}

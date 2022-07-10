using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an abstract input MIDI device. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_input">Input device</see> articles.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// Occurs when a MIDI event is received.
        /// </summary>
        event EventHandler<MidiEventReceivedEventArgs> EventReceived;

        /// <summary>
        /// Gets a value that indicates whether the current input device is currently listening for
        /// incoming MIDI events.
        /// </summary>
        bool IsListeningForEvents { get; }

        /// <summary>
        /// Starts listening for incoming MIDI events on the current input device.
        /// </summary>
        void StartEventsListening();

        /// <summary>
        /// Stops listening for incoming MIDI events on the current input device.
        /// </summary>
        void StopEventsListening();
    }
}

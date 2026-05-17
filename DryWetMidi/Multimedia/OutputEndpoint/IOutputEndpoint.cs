using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an abstract output MIDI endpoint. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_output">Output endpoint</see> articles.
    /// </summary>
    public interface IOutputEndpoint : IDisposable
    {
        /// <summary>
        /// Occurs when a MIDI event is sent.
        /// </summary>
        event EventHandler<MidiEventSentEventArgs> EventSent;

        // TODO: docs on this method. It is needed to call it before sending events to the endpoint
        /// <summary>
        /// Prepares output MIDI endpoint for sending events to it allocating necessary
        /// resources.
        /// </summary>
        /// <remarks>
        /// It is not needed to call this method before actual MIDI data
        /// sending since first call of <see cref="SendEvent(MidiEvent)"/> will prepare
        /// the endpoint automatically. But it can take some time so you may decide
        /// to call <see cref="PrepareForEventsSending"/> before working with endpoint.
        /// </remarks>
        void PrepareForEventsSending();

        /// <summary>
        /// Sends a MIDI event to the current output endpoint.
        /// </summary>
        /// <param name="midiEvent">MIDI event to send.</param>
        void SendEvent(MidiEvent midiEvent);
    }
}

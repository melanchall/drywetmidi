using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for the <see cref="IInputEndpoint.EventReceived"/> event.
    /// </summary>
    public sealed class MidiEventReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventReceivedEventArgs"/> with
        /// the specified MIDI event.
        /// </summary>
        /// <param name="midiEvent">MIDI event received by <see cref="IInputEndpoint"/>.</param>
        /// <param name="timestamp">Timestamp of the MIDI event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public MidiEventReceivedEventArgs(MidiEvent midiEvent, long timestamp)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            Event = midiEvent;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets MIDI event received by <see cref="IInputEndpoint"/>.
        /// </summary>
        public MidiEvent Event { get; }
        
        public long Timestamp { get; }

        public override string ToString() =>
            $"{Event} received at {Timestamp}";
    }
}

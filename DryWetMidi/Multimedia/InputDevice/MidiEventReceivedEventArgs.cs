using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for the <see cref="IInputDevice.EventReceived"/> event.
    /// </summary>
    public sealed class MidiEventReceivedEventArgs : EventArgs
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventReceivedEventArgs"/> with
        /// the specified MIDI event.
        /// </summary>
        /// <param name="midiEvent">MIDI event received by <see cref="IInputDevice"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public MidiEventReceivedEventArgs(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets MIDI event received by <see cref="IInputDevice"/>.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

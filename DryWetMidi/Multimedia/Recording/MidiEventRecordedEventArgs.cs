using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds an instance of <see cref="MidiEvent"/> for <see cref="Recording.EventRecorded"/> event.
    /// </summary>
    public sealed class MidiEventRecordedEventArgs
    {
        #region Constructor

        internal MidiEventRecordedEventArgs(MidiEvent midiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the recorded MIDI event.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion
    }
}

using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// A class representing events that occur as part of the playback process.
    /// </summary>
    public sealed class PlaybackEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackEvent"/> class.
        /// </summary>
        /// <param name="midiEvent">The underlying MIDI event.</param>
        /// <param name="time">The time between the start of playback and the current event time.</param>
        /// <param name="rawTime">The raw timestamp at which the event occurred.</param>
        public PlaybackEvent(MidiEvent midiEvent, TimeSpan time, long rawTime)
        {
            Event = midiEvent;
            Time = time;
            RawTime = rawTime;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the underlying MIDI event.
        /// </summary>
        public MidiEvent Event { get; }

        /// <summary>
        /// Gets the time between the start of playback and the current event time.
        /// </summary>
        public TimeSpan Time { get; }

        /// <summary>
        /// Gets the raw timestamp at which the event occurred.
        /// </summary>
        public long RawTime { get; }

        /// <summary>
        /// Gets the current event's metadata.
        /// </summary>
        public PlaybackEventMetadata Metadata { get; } = new PlaybackEventMetadata();

        #endregion
    }
}

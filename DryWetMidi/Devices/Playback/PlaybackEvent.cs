using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class PlaybackEvent
    {
        #region Constructor

        public PlaybackEvent(MidiEvent midiEvent, TimeSpan time, long rawTime)
        {
            Event = midiEvent;
            Time = time;
            RawTime = rawTime;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        public long RawTime { get; }

        public ICollection<IPlaybackEventTag> Tags { get; } = new List<IPlaybackEventTag>();

        #endregion
    }
}

using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class TimestampedEvent
    {
        public TimestampedEvent(MidiEvent midiEvent, TimeSpan time)
            : this(midiEvent, time, TimeSpan.Zero)
        {
        }

        public TimestampedEvent(MidiEvent midiEvent, TimeSpan time, TimeSpan receivedTimestamp)
        {
            Event = midiEvent;
            Time = time;
            ReceivedTimestamp = receivedTimestamp;
        }

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        public TimeSpan ReceivedTimestamp { get; }

        public long DelayMs { get; set; }

        public override string ToString() =>
            $"{Time}{(DelayMs > 0 ? $" + {DelayMs}ms" : null)}: {Event}";
    }
}

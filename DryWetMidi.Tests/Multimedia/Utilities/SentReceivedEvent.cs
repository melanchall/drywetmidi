using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class SentReceivedEvent
    {
        #region Constructor

        public SentReceivedEvent(MidiEvent midiEvent, TimeSpan time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        public long DelayMs { get; set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Time}{(DelayMs > 0 ? $" + {DelayMs}ms" : null)}: {Event}";
        }

        #endregion
    }
}

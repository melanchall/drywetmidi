using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    internal sealed class ReceivedEvent
    {
        #region Constructor

        public ReceivedEvent(MidiEvent midiEvent, TimeSpan time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Event} at {Time}";
        }

        #endregion
    }
}

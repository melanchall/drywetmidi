using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    internal sealed class EventToSend
    {
        #region Constructor

        public EventToSend(MidiEvent midiEvent, TimeSpan delay)
        {
            Event = midiEvent;
            Delay = delay;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Delay { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Event} after {Delay} of delay";
        }

        #endregion
    }
}

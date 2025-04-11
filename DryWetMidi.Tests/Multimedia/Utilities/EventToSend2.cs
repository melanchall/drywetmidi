using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class EventToSend2
    {
        #region Constructor

        public EventToSend2(MidiEvent midiEvent, TimeSpan time)
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
            return $"{Time}: {Event}";
        }

        #endregion
    }
}

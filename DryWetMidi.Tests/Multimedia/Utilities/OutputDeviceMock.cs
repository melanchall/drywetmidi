using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class OutputDeviceMock : IOutputDevice
    {
        public event EventHandler<MidiEventSentEventArgs> EventSent;

        public void PrepareForEventsSending()
        {
        }

        public void SendEvent(MidiEvent midiEvent)
        {
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }

        public void Dispose()
        {
        }
    }
}

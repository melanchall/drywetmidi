namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemRealTimeEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
            {
                var statusByte = GetStatusByte(midiEvent);
                writer.WriteByte(statusByte);
            }

            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            switch (midiEvent.EventType)
            {
                case MidiEventType.ActiveSensing:
                    return EventStatusBytes.SystemRealTime.ActiveSensing;
                case MidiEventType.Continue:
                    return EventStatusBytes.SystemRealTime.Continue;
                case MidiEventType.Reset:
                    return EventStatusBytes.SystemRealTime.Reset;
                case MidiEventType.Start:
                    return EventStatusBytes.SystemRealTime.Start;
                case MidiEventType.Stop:
                    return EventStatusBytes.SystemRealTime.Stop;
                case MidiEventType.TimingClock:
                    return EventStatusBytes.SystemRealTime.TimingClock;
            }

            return 0;
        }

        #endregion
    }
}

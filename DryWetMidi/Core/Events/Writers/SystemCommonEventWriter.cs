namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemCommonEventWriter : IEventWriter
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
                case MidiEventType.MidiTimeCode:
                    return EventStatusBytes.SystemCommon.MtcQuarterFrame;
                case MidiEventType.SongSelect:
                    return EventStatusBytes.SystemCommon.SongSelect;
                case MidiEventType.SongPositionPointer:
                    return EventStatusBytes.SystemCommon.SongPositionPointer;
                case MidiEventType.TuneRequest:
                    return EventStatusBytes.SystemCommon.TuneRequest;
            }

            return 0;
        }

        #endregion
    }
}

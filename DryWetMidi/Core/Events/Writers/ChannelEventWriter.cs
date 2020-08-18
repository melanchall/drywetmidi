using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class ChannelEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
            {
                var statusByte = GetStatusByte(midiEvent);
                writer.WriteByte(statusByte);
            }

            //

            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            byte statusByte = 0;

            switch (midiEvent.EventType)
            {
                case MidiEventType.NoteOff:
                    statusByte = EventStatusBytes.Channel.NoteOff;
                    break;
                case MidiEventType.NoteOn:
                    statusByte = EventStatusBytes.Channel.NoteOn;
                    break;
                case MidiEventType.ControlChange:
                    statusByte = EventStatusBytes.Channel.ControlChange;
                    break;
                case MidiEventType.PitchBend:
                    statusByte = EventStatusBytes.Channel.PitchBend;
                    break;
                case MidiEventType.ChannelAftertouch:
                    statusByte = EventStatusBytes.Channel.ChannelAftertouch;
                    break;
                case MidiEventType.ProgramChange:
                    statusByte = EventStatusBytes.Channel.ProgramChange;
                    break;
                case MidiEventType.NoteAftertouch:
                    statusByte = EventStatusBytes.Channel.NoteAftertouch;
                    break;
            }

            var channel = ((ChannelEvent)midiEvent).Channel;
            return DataTypesUtilities.CombineAsFourBitNumbers(statusByte, channel);
        }

        #endregion
    }
}

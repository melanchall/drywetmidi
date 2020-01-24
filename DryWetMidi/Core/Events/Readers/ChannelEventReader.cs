using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class ChannelEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = currentStatusByte.GetHead();
            var channel = currentStatusByte.GetTail();

            ChannelEvent channelEvent = null;

            switch (statusByte)
            {
                case EventStatusBytes.Channel.NoteOff:
                    channelEvent = new NoteOffEvent();
                    break;
                case EventStatusBytes.Channel.NoteOn:
                    channelEvent = new NoteOnEvent();
                    break;
                case EventStatusBytes.Channel.ControlChange:
                    channelEvent = new ControlChangeEvent();
                    break;
                case EventStatusBytes.Channel.PitchBend:
                    channelEvent = new PitchBendEvent();
                    break;
                case EventStatusBytes.Channel.ChannelAftertouch:
                    channelEvent = new ChannelAftertouchEvent();
                    break;
                case EventStatusBytes.Channel.ProgramChange:
                    channelEvent = new ProgramChangeEvent();
                    break;
                case EventStatusBytes.Channel.NoteAftertouch:
                    channelEvent = new NoteAftertouchEvent();
                    break;
                default:
                    throw new UnknownChannelEventException(statusByte, channel);
            }

            channelEvent.Read(reader, settings, MidiEvent.UnknownContentSize);
            channelEvent.Channel = channel;

            var noteOnEvent = channelEvent as NoteOnEvent;
            if (noteOnEvent != null && settings.SilentNoteOnPolicy == SilentNoteOnPolicy.NoteOff && noteOnEvent.Velocity == 0)
                channelEvent = new NoteOffEvent
                {
                    DeltaTime = noteOnEvent.DeltaTime,
                    Channel = noteOnEvent.Channel,
                    NoteNumber = noteOnEvent.NoteNumber
                };

            return channelEvent;
        }

        #endregion
    }
}

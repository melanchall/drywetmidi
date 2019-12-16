using System;
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

            Type eventType;
            if (!StandardEventTypes.Channel.TryGetType(statusByte, out eventType))
                throw new UnknownChannelEventException(statusByte, channel);

            var channelEvent = (ChannelEvent)Activator.CreateInstance(eventType);
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

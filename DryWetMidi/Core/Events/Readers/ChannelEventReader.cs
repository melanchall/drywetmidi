using System;
using Melanchall.DryWetMidi.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class ChannelEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = currentStatusByte.GetHead();
            var channel = currentStatusByte.GetTail();

            var channelEvent = CreateEvent(statusByte);
            if (channelEvent == null)
            {
                ReactOnUnknownChannelEvent(statusByte, channel, reader, settings);
                return null;
            }

            channelEvent.Read(reader, settings, MidiEvent.UnknownContentSize);
            channelEvent.Channel = channel;

            return ProcessSilentNoteOn(channelEvent, settings);
        }

        public async Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken)
        {
            var statusByte = currentStatusByte.GetHead();
            var channel = currentStatusByte.GetTail();

            var channelEvent = CreateEvent(statusByte);
            if (channelEvent == null)
            {
                var bytesToSkip = GetUnknownEventBytesToSkip(statusByte, channel, settings);
                if (bytesToSkip != 0)
                    await reader.SetPositionAsync(reader.Position + bytesToSkip, cancellationToken).ConfigureAwait(false);

                return null;
            }

            await channelEvent.ReadAsync(reader, settings, MidiEvent.UnknownContentSize, cancellationToken).ConfigureAwait(false);
            channelEvent.Channel = channel;

            return ProcessSilentNoteOn(channelEvent, settings);
        }

        #endregion

        #region Methods

        private static ChannelEvent? CreateEvent(FourBitNumber statusByte)
        {
            switch (statusByte)
            {
                case EventStatusBytes.Channel.NoteOff:
                    return new NoteOffEvent();
                case EventStatusBytes.Channel.NoteOn:
                    return new NoteOnEvent();
                case EventStatusBytes.Channel.ControlChange:
                    return new ControlChangeEvent();
                case EventStatusBytes.Channel.PitchBend:
                    return new PitchBendEvent();
                case EventStatusBytes.Channel.ChannelAftertouch:
                    return new ChannelAftertouchEvent();
                case EventStatusBytes.Channel.ProgramChange:
                    return new ProgramChangeEvent();
                case EventStatusBytes.Channel.NoteAftertouch:
                    return new NoteAftertouchEvent();
                default:
                    return null;
            }
        }

        private static ChannelEvent ProcessSilentNoteOn(ChannelEvent channelEvent, ReadingSettings settings)
        {
            if (channelEvent.EventType == MidiEventType.NoteOn)
            {
                var noteOnEvent = (NoteOnEvent)channelEvent;
                if (settings.SilentNoteOnPolicy == SilentNoteOnPolicy.NoteOff && noteOnEvent.Velocity == 0)
                    channelEvent = new NoteOffEvent
                    {
                        DeltaTime = noteOnEvent.DeltaTime,
                        Channel = noteOnEvent.Channel,
                        NoteNumber = noteOnEvent.NoteNumber
                    };
            }

            return channelEvent;
        }

        private void ReactOnUnknownChannelEvent(FourBitNumber statusByte, FourBitNumber channel, MidiReader reader, ReadingSettings settings)
        {
            var bytesToSkip = GetUnknownEventBytesToSkip(statusByte, channel, settings);
            if (bytesToSkip != 0)
                reader.Position += bytesToSkip;
        }

        private static int GetUnknownEventBytesToSkip(
            FourBitNumber statusByte,
            FourBitNumber channel,
            ReadingSettings settings)
        {
            switch (settings.UnknownChannelEventPolicy)
            {
                case UnknownChannelEventPolicy.Abort:
                    throw new UnknownChannelEventException(statusByte, channel);
                case UnknownChannelEventPolicy.SkipStatusByte:
                    return 0;
                case UnknownChannelEventPolicy.SkipStatusByteAndOneDataByte:
                    return 1;
                case UnknownChannelEventPolicy.SkipStatusByteAndTwoDataBytes:
                    return 2;
                case UnknownChannelEventPolicy.UseCallback:
                    var callback = settings.UnknownChannelEventCallback;
                    if (callback == null)
                        throw new InvalidOperationException("Unknown channel event callback is not set.");

                    var action = callback(statusByte, channel);
                    switch (action.Instruction)
                    {
                        case UnknownChannelEventInstruction.Abort:
                            throw new UnknownChannelEventException(statusByte, channel);
                        case UnknownChannelEventInstruction.SkipData:
                            return action.DataBytesToSkipCount;
                    }
                    break;
            }

            return 0;
        }

        #endregion
    }
}

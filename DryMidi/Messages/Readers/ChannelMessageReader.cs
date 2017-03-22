using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    public sealed class ChannelMessageReader : IMessageReader
    {
        #region Constants

        private static readonly Dictionary<byte, Type> _messageTypes = new Dictionary<byte, Type>
        {
            [MessagesStatusBytes.Channel.ChannelAftertouch] = typeof(ChannelAftertouchMessage),
            [MessagesStatusBytes.Channel.ControlChange] = typeof(ControlChangeMessage),
            [MessagesStatusBytes.Channel.NoteAftertouch] = typeof(NoteAftertouchMessage),
            [MessagesStatusBytes.Channel.NoteOff] = typeof(NoteOffMessage),
            [MessagesStatusBytes.Channel.NoteOn] = typeof(NoteOnMessage),
            [MessagesStatusBytes.Channel.PitchBend] = typeof(PitchBendMessage),
            [MessagesStatusBytes.Channel.ProgramChange] = typeof(ProgramChangeMessage)
        };

        #endregion

        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = currentStatusByte.GetHead();
            var channel = currentStatusByte.GetTail();

            Type messageType;
            if (!_messageTypes.TryGetValue(statusByte, out messageType))
                throw new UnknownChannelMessageException(statusByte, channel);

            var message = (ChannelMessage)Activator.CreateInstance(messageType);
            message.ReadContent(reader, settings);
            message.Channel = channel;
            return message;
        }

        #endregion
    }
}

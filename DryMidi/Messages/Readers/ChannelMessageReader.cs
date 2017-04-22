using System;

namespace Melanchall.DryMidi
{
    internal sealed class ChannelMessageReader : IMessageReader
    {
        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = currentStatusByte.GetHead();
            var channel = currentStatusByte.GetTail();

            Type messageType;
            if (!StandardMessageTypes.Channel.TryGetType(statusByte, out messageType))
                throw new UnknownChannelMessageException(statusByte, channel);

            var message = (ChannelMessage)Activator.CreateInstance(messageType);
            message.ReadContent(reader, settings, Message.UnknownContentSize);
            message.Channel = channel;
            return message;
        }

        #endregion
    }
}

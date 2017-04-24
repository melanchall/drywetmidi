using System;

namespace Melanchall.DryMidi
{
    internal sealed class MetaMessageWriter : IMessageWriter
    {
        #region IMessageWriter

        public void Write(Message message, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is MetaMessage))
                throw new ArgumentException("Message is not Meta message.", nameof(message));

            //

            if (writeStatusByte)
                writer.WriteByte(MessageStatusBytes.Global.Meta);

            //

            byte statusByte;

            var unknownMetaMessage = message as UnknownMetaMessage;
            if (unknownMetaMessage != null)
                statusByte = unknownMetaMessage.StatusByte;
            else
            {
                var messageType = message.GetType();
                if (!StandardMessageTypes.Meta.TryGetStatusByte(messageType, out statusByte) && settings.CustomMetaMessageTypes?.TryGetStatusByte(messageType, out statusByte) != true)
                    throw new InvalidOperationException($"Unable to write the {messageType} message.");
            }

            writer.WriteByte(statusByte);

            //

            var contentSize = message.GetContentSize();
            writer.WriteVlqNumber(contentSize);
            message.WriteContent(writer, settings);
        }

        public int CalculateSize(Message message, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is MetaMessage))
                throw new ArgumentException("Message is not Meta message.", nameof(message));

            //

            var contentSize = message.GetContentSize();
            return (writeStatusByte ? 1 : 0) + 1 + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(Message message)
        {
            return MessageStatusBytes.Global.Meta;
        }

        #endregion
    }
}

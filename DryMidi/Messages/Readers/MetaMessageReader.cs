using System;

namespace Melanchall.DryMidi
{
    internal sealed class MetaMessageReader : IMessageReader
    {
        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = reader.ReadByte();
            var size = reader.ReadVlqNumber();

            //

            Type messageType;
            var message = TryGetMessageType(settings.CustomMetaMessagesTypes, statusByte, out messageType)
                ? (MetaMessage)Activator.CreateInstance(messageType)
                : new UnknownMetaMessage(statusByte);

            //

            var readerPosition = reader.Position;
            message.ReadContent(reader, settings, size);

            var bytesRead = reader.Position - readerPosition;
            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
                reader.ReadBytes((int)bytesUnread);

            return message;
        }

        #endregion

        #region Methods

        private static bool TryGetMessageType(MessageTypesCollection customMetaMessageTypes, byte statusByte, out Type messageType)
        {
            return StandardMessageTypes.Meta.TryGetType(statusByte, out messageType) ||
                   (customMetaMessageTypes?.TryGetType(statusByte, out messageType) == true && IsMetaMessageType(messageType));
        }

        private static bool IsMetaMessageType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MetaMessage)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}

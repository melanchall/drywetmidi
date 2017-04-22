using System;

namespace Melanchall.DryMidi
{
    internal sealed class SysExMessageReader : IMessageReader
    {
        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var size = reader.ReadVlqNumber();

            //

            Type messageType;
            var message = StandardMessageTypes.SysEx.TryGetType(currentStatusByte, out messageType)
                ? (SysExMessage)Activator.CreateInstance(messageType)
                : null;

            if (message == null)
                throw new InvalidOperationException("Unknown SysEx message.");

            //

            message.Completed = false;
            message.ReadContent(reader, settings, size);
            return message;
        }

        #endregion
    }
}

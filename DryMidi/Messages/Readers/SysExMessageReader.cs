using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    internal sealed class SysExMessageReader : IMessageReader
    {
        #region Constants

        private static readonly Dictionary<byte, Type> _messageTypes = new Dictionary<byte, Type>
        {
            [MessagesStatusBytes.Global.EscapeSysEx] = typeof(EscapeSysExMessage),
            [MessagesStatusBytes.Global.NormalSysEx] = typeof(NormalSysExMessage)
        };

        #endregion

        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var size = reader.ReadVlqNumber();

            //

            Type messageType;
            var message = _messageTypes.TryGetValue(currentStatusByte, out messageType)
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

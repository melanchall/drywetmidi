using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    internal sealed class SysExMessageWriter : IMessageWriter
    {
        #region Constants

        private static readonly Dictionary<Type, byte> _messageStatusBytes = new Dictionary<Type, byte>
        {
            [typeof(EscapeSysExMessage)] = MessagesStatusBytes.Global.EscapeSysEx,
            [typeof(NormalSysExMessage)] = MessagesStatusBytes.Global.NormalSysEx
        };

        #endregion

        #region IMessageWriter

        public void Write(Message message, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var sysExMessage = message as SysExMessage;
            if (sysExMessage == null)
                throw new ArgumentException("Message is not SysEx message.", nameof(message));

            //

            if (writeStatusByte)
            {
                byte statusByte;
                if (!_messageStatusBytes.TryGetValue(message.GetType(), out statusByte))
                    throw new Exception();

                writer.WriteByte(statusByte);
            }

            //

            var contentSize = message.GetContentSize();
            writer.WriteVlqNumber(contentSize);
            message.WriteContent(writer, settings);
        }

        public int CalculateSize(Message message, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is SysExMessage))
                throw new ArgumentException("Message is not SysEx message.", nameof(message));

            //

            var contentSize = message.GetContentSize();
            return (writeStatusByte ? 1 : 0) + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is SysExMessage))
                throw new ArgumentException("Message is not SysEx message.", nameof(message));

            //

            byte statusByte;
            if (!_messageStatusBytes.TryGetValue(message.GetType(), out statusByte))
                throw new Exception();

            return statusByte;
        }

        #endregion
    }
}

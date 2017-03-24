using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Melanchall.DryMidi
{
    internal sealed class ChannelMessageWriter : IMessageWriter
    {
        #region Constants

        private static readonly Dictionary<Type, byte> _messageStatusBytes = new Dictionary<Type, byte>
        {
            [typeof(ChannelAftertouchMessage)] = MessagesStatusBytes.Channel.ChannelAftertouch,
            [typeof(ControlChangeMessage)]     = MessagesStatusBytes.Channel.ControlChange,
            [typeof(NoteAftertouchMessage)]    = MessagesStatusBytes.Channel.NoteAftertouch,
            [typeof(NoteOffMessage)]           = MessagesStatusBytes.Channel.NoteOff,
            [typeof(NoteOnMessage)]            = MessagesStatusBytes.Channel.NoteOn,
            [typeof(PitchBendMessage)]         = MessagesStatusBytes.Channel.PitchBend,
            [typeof(ProgramChangeMessage)]     = MessagesStatusBytes.Channel.ProgramChange
        };

        #endregion

        #region IMessageWriter

        public void Write(Message message, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var channelMessage = message as ChannelMessage;
            if (channelMessage == null)
                throw new ArgumentException("Message is not Channel message.", nameof(message));

            //

            if (writeStatusByte)
            {
                byte statusByte;
                if (!_messageStatusBytes.TryGetValue(message.GetType(), out statusByte))
                    Debug.Fail($"No status byte defined for {message.GetType()}.");

                var channel = channelMessage.Channel;

                var totalStatusByte = DataTypesUtilities.Combine((FourBitNumber)statusByte, channel);
                writer.WriteByte(totalStatusByte);
            }

            //

            message.WriteContent(writer, settings);
        }

        public int CalculateSize(Message message, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is ChannelMessage))
                throw new ArgumentException("Message is not Channel message.", nameof(message));

            //

            return (writeStatusByte ? 1 : 0) + message.GetContentSize();
        }

        public byte GetStatusByte(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var channelMessage = message as ChannelMessage;
            if (channelMessage == null)
                throw new ArgumentException("Message is not Channel message.", nameof(message));

            //

            byte statusByte;
            if (!_messageStatusBytes.TryGetValue(message.GetType(), out statusByte))
                Debug.Fail($"No status byte defined for {message.GetType()}.");

            var channel = channelMessage.Channel;

            return DataTypesUtilities.Combine((FourBitNumber)statusByte, channel);
        }

        #endregion
    }
}

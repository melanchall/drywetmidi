using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    internal sealed class MetaMessageWriter : IMessageWriter
    {
        #region Constants

        private static readonly Dictionary<Type, byte> _messageStatusBytes = new Dictionary<Type, byte>
        {
            [typeof(SequenceNumberMessage)]    = MessagesStatusBytes.Meta.SequenceNumber,
            [typeof(TextMessage)]              = MessagesStatusBytes.Meta.Text,
            [typeof(CopyrightNoticeMessage)]   = MessagesStatusBytes.Meta.CopyrightNotice,
            [typeof(SequenceTrackNameMessage)] = MessagesStatusBytes.Meta.SequenceTrackName,
            [typeof(InstrumentNameMessage)]    = MessagesStatusBytes.Meta.InstrumentName,
            [typeof(LyricMessage)]             = MessagesStatusBytes.Meta.Lyric,
            [typeof(MarkerMessage)]            = MessagesStatusBytes.Meta.Marker,
            [typeof(CuePointMessage)]          = MessagesStatusBytes.Meta.CuePoint,
            [typeof(ProgramNameMessage)]       = MessagesStatusBytes.Meta.ProgramName,
            [typeof(DeviceNameMessage)]        = MessagesStatusBytes.Meta.DeviceName,
            [typeof(ChannelPrefixMessage)]     = MessagesStatusBytes.Meta.ChannelPrefix,
            [typeof(PortPrefixMessage)]        = MessagesStatusBytes.Meta.PortPrefix,
            [typeof(EndOfTrackMessage)]        = MessagesStatusBytes.Meta.EndOfTrack,
            [typeof(SetTempoMessage)]          = MessagesStatusBytes.Meta.SetTempo,
            [typeof(SmpteOffsetMessage)]       = MessagesStatusBytes.Meta.SmpteOffset,
            [typeof(TimeSignatureMessage)]     = MessagesStatusBytes.Meta.TimeSignature,
            [typeof(KeySignatureMessage)]      = MessagesStatusBytes.Meta.KeySignature,
            [typeof(SequencerSpecificMessage)] = MessagesStatusBytes.Meta.SequencerSpecific
        };

        #endregion

        #region IMessageWriter

        public void Write(Message message, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!(message is MetaMessage))
                throw new ArgumentException("Message is not Meta message.", nameof(message));

            //

            if (writeStatusByte)
                writer.WriteByte(MessagesStatusBytes.Global.Meta);

            //

            byte statusByte;

            var unknownMetaMessage = message as UnknownMetaMessage;
            if (unknownMetaMessage != null)
                statusByte = unknownMetaMessage.StatusByte;
            else if (!_messageStatusBytes.TryGetValue(message.GetType(), out statusByte))
                throw new NotImplementedException($"Writing of the {message.GetType()} is not implemented.");

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
            return MessagesStatusBytes.Global.Meta;
        }

        #endregion
    }
}

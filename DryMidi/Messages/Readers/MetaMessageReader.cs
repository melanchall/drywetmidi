using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    public sealed class MetaMessageReader : IMessageReader
    {
        #region Constants

        private static readonly Dictionary<byte, Type> _messageTypes = new Dictionary<byte, Type>
        {
            [MessagesStatusBytes.Meta.SequenceNumber]    = typeof(SequenceNumberMessage),
            [MessagesStatusBytes.Meta.Text]              = typeof(TextMessage),
            [MessagesStatusBytes.Meta.CopyrightNotice]   = typeof(CopyrightNoticeMessage),
            [MessagesStatusBytes.Meta.SequenceTrackName] = typeof(SequenceTrackNameMessage),
            [MessagesStatusBytes.Meta.InstrumentName]    = typeof(InstrumentNameMessage),
            [MessagesStatusBytes.Meta.Lyric]             = typeof(LyricMessage),
            [MessagesStatusBytes.Meta.Marker]            = typeof(MarkerMessage),
            [MessagesStatusBytes.Meta.CuePoint]          = typeof(CuePointMessage),
            [MessagesStatusBytes.Meta.ProgramName]       = typeof(ProgramNameMessage),
            [MessagesStatusBytes.Meta.DeviceName]        = typeof(DeviceNameMessage),
            [MessagesStatusBytes.Meta.ChannelPrefix]     = typeof(ChannelPrefixMessage),
            [MessagesStatusBytes.Meta.PortPrefix]        = typeof(PortPrefixMessage),
            [MessagesStatusBytes.Meta.EndOfTrack]        = typeof(EndOfTrackMessage),
            [MessagesStatusBytes.Meta.SetTempo]          = typeof(SetTempoMessage),
            [MessagesStatusBytes.Meta.SmpteOffset]       = typeof(SmpteOffsetMessage),
            [MessagesStatusBytes.Meta.TimeSignature]     = typeof(TimeSignatureMessage),
            [MessagesStatusBytes.Meta.KeySignature]      = typeof(KeySignatureMessage),
            [MessagesStatusBytes.Meta.SequencerSpecific] = typeof(SequencerSpecificMessage)
        };

        #endregion

        #region IMessageReader

        public Message Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = reader.ReadByte();
            var size = reader.ReadVlqNumber();

            //

            Type messageType;
            var message = _messageTypes.TryGetValue(statusByte, out messageType)
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
    }
}

using System;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MetaEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = reader.ReadByte();
            var size = reader.ReadVlqNumber();

            //

            MetaEvent metaEvent;

            switch (statusByte)
            {
                case EventStatusBytes.Meta.Lyric:
                    metaEvent = new LyricEvent();
                    break;
                case EventStatusBytes.Meta.SetTempo:
                    metaEvent = new SetTempoEvent();
                    break;
                case EventStatusBytes.Meta.Text:
                    metaEvent = new TextEvent();
                    break;
                case EventStatusBytes.Meta.SequenceTrackName:
                    metaEvent = new SequenceTrackNameEvent();
                    break;
                case EventStatusBytes.Meta.PortPrefix:
                    metaEvent = new PortPrefixEvent();
                    break;
                case EventStatusBytes.Meta.TimeSignature:
                    metaEvent = new TimeSignatureEvent();
                    break;
                case EventStatusBytes.Meta.SequencerSpecific:
                    metaEvent = new SequencerSpecificEvent();
                    break;
                case EventStatusBytes.Meta.KeySignature:
                    metaEvent = new KeySignatureEvent();
                    break;
                case EventStatusBytes.Meta.Marker:
                    metaEvent = new MarkerEvent();
                    break;
                case EventStatusBytes.Meta.ChannelPrefix:
                    metaEvent = new ChannelPrefixEvent();
                    break;
                case EventStatusBytes.Meta.InstrumentName:
                    metaEvent = new InstrumentNameEvent();
                    break;
                case EventStatusBytes.Meta.CopyrightNotice:
                    metaEvent = new CopyrightNoticeEvent();
                    break;
                case EventStatusBytes.Meta.SmpteOffset:
                    metaEvent = new SmpteOffsetEvent();
                    break;
                case EventStatusBytes.Meta.DeviceName:
                    metaEvent = new DeviceNameEvent();
                    break;
                case EventStatusBytes.Meta.CuePoint:
                    metaEvent = new CuePointEvent();
                    break;
                case EventStatusBytes.Meta.ProgramName:
                    metaEvent = new ProgramNameEvent();
                    break;
                case EventStatusBytes.Meta.SequenceNumber:
                    metaEvent = new SequenceNumberEvent();
                    break;
                case EventStatusBytes.Meta.EndOfTrack:
                    metaEvent = new EndOfTrackEvent();
                    break;
                default:
                    {
                        Type eventType = null;
                        metaEvent = settings.CustomMetaEventTypes?.TryGetType(statusByte, out eventType) == true && IsMetaEventType(eventType)
                            ? (MetaEvent)Activator.CreateInstance(eventType)
                            : new UnknownMetaEvent(statusByte);
                    }
                    break;
            }

            //

            var readerPosition = reader.Position;
            metaEvent.Read(reader, settings, size);

            var bytesRead = reader.Position - readerPosition;
            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
                reader.Position += bytesUnread;

            return metaEvent;
        }

        #endregion

        #region Methods

        private static bool IsMetaEventType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MetaEvent)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}

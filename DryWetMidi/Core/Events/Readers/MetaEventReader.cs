using Melanchall.DryWetMidi.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MetaEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = reader.ReadByte();
            var size = reader.ReadVlqNumber();

            var metaEvent = CreateEvent(statusByte, settings);
            var readerPosition = reader.Position;
            metaEvent.Read(reader, settings, size);

            var bytesRead = reader.Position - readerPosition;
            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
                reader.Position = Math.Min(reader.Position + bytesUnread, reader.Length);

            return metaEvent;
        }

        public async Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken)
        {
            var statusByte = await reader.ReadByteAsync(cancellationToken).ConfigureAwait(false);
            var size = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);

            var metaEvent = CreateEvent(statusByte, settings);
            var readerPosition = reader.Position;
            await metaEvent.ReadAsync(reader, settings, size, cancellationToken).ConfigureAwait(false);

            var bytesRead = reader.Position - readerPosition;
            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
            {
                await reader.SetPositionAsync(
                    Math.Min(reader.Position + bytesUnread, reader.Length),
                    cancellationToken).ConfigureAwait(false);
            }

            return metaEvent;
        }

        #endregion

        #region Methods

        private static MetaEvent CreateEvent(byte statusByte, ReadingSettings settings)
        {
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
                        if (settings.CustomMetaEventTypes?.TryGetType(statusByte, out var eventType) == true && IsMetaEventType(eventType))
                        {
                            // TODO: get rid of Activator and reflection at all
                            var metaEventObject = Activator.CreateInstance(eventType);

                            // TODO: exception class
                            if (metaEventObject == null)
                                throw new InvalidOperationException("AAA");

                            metaEvent = (MetaEvent)metaEventObject;
                        }
                        else
                        {
                            metaEvent = new UnknownMetaEvent(statusByte);
                        }
                    }
                    break;
            }

            return metaEvent;
        }

        private static bool IsMetaEventType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MetaEvent)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}

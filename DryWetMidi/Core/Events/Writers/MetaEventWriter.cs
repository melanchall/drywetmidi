using Melanchall.DryWetMidi.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MetaEventWriter : IEventWriter
    {
        #region IEventWriter

        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Custom meta event types are guaranteed to have public parameterless constructors when registered via CustomMetaEventTypes.Add.")]
        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
                writer.WriteByte(EventStatusBytes.Global.Meta);

            writer.WriteByte(GetMetaStatusByte(midiEvent, settings));

            var contentSize = midiEvent.GetSize(settings);
            writer.WriteVlqNumber(contentSize);
            midiEvent.Write(writer, settings);
        }

        public async Task WriteAsync(
            MidiEvent midiEvent,
            MidiWriter writer,
            WritingSettings settings,
            bool writeStatusByte,
            CancellationToken cancellationToken)
        {
            if (writeStatusByte)
                await writer.WriteByteAsync(EventStatusBytes.Global.Meta, cancellationToken).ConfigureAwait(false);

            await writer.WriteByteAsync(GetMetaStatusByte(midiEvent, settings), cancellationToken).ConfigureAwait(false);

            var contentSize = midiEvent.GetSize(settings);
            await writer.WriteVlqNumberAsync(contentSize, cancellationToken).ConfigureAwait(false);
            await midiEvent.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            var contentSize = midiEvent.GetSize(settings);
            return (writeStatusByte ? 1 : 0) + 1 + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            return EventStatusBytes.Global.Meta;
        }

        #endregion

        #region Methods

        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Custom meta event types are guaranteed to have public parameterless constructors when registered via CustomMetaEventTypes.Add.")]
        private static byte GetMetaStatusByte(MidiEvent midiEvent, WritingSettings settings)
        {
            byte statusByte = 0;

            switch (midiEvent.EventType)
            {
                case MidiEventType.Lyric:
                    statusByte = EventStatusBytes.Meta.Lyric;
                    break;
                case MidiEventType.SetTempo:
                    statusByte = EventStatusBytes.Meta.SetTempo;
                    break;
                case MidiEventType.Text:
                    statusByte = EventStatusBytes.Meta.Text;
                    break;
                case MidiEventType.SequenceTrackName:
                    statusByte = EventStatusBytes.Meta.SequenceTrackName;
                    break;
                case MidiEventType.PortPrefix:
                    statusByte = EventStatusBytes.Meta.PortPrefix;
                    break;
                case MidiEventType.TimeSignature:
                    statusByte = EventStatusBytes.Meta.TimeSignature;
                    break;
                case MidiEventType.SequencerSpecific:
                    statusByte = EventStatusBytes.Meta.SequencerSpecific;
                    break;
                case MidiEventType.KeySignature:
                    statusByte = EventStatusBytes.Meta.KeySignature;
                    break;
                case MidiEventType.Marker:
                    statusByte = EventStatusBytes.Meta.Marker;
                    break;
                case MidiEventType.ChannelPrefix:
                    statusByte = EventStatusBytes.Meta.ChannelPrefix;
                    break;
                case MidiEventType.InstrumentName:
                    statusByte = EventStatusBytes.Meta.InstrumentName;
                    break;
                case MidiEventType.CopyrightNotice:
                    statusByte = EventStatusBytes.Meta.CopyrightNotice;
                    break;
                case MidiEventType.SmpteOffset:
                    statusByte = EventStatusBytes.Meta.SmpteOffset;
                    break;
                case MidiEventType.DeviceName:
                    statusByte = EventStatusBytes.Meta.DeviceName;
                    break;
                case MidiEventType.CuePoint:
                    statusByte = EventStatusBytes.Meta.CuePoint;
                    break;
                case MidiEventType.ProgramName:
                    statusByte = EventStatusBytes.Meta.ProgramName;
                    break;
                case MidiEventType.SequenceNumber:
                    statusByte = EventStatusBytes.Meta.SequenceNumber;
                    break;
                case MidiEventType.EndOfTrack:
                    statusByte = EventStatusBytes.Meta.EndOfTrack;
                    break;
                case MidiEventType.UnknownMeta:
                    statusByte = ((UnknownMetaEvent)midiEvent).StatusByte;
                    break;
                default:
                    {
                        var eventType = midiEvent.GetType();
                        if (settings.CustomMetaEventTypes?.TryGetStatusByte(eventType, out statusByte) != true)
                            Debug.Fail($"Unable to write the {eventType} event.");
                    }
                    break;
            }

            return statusByte;
        }

        #endregion
    }
}

using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MetaEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
                writer.WriteByte(EventStatusBytes.Global.Meta);

            //

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

            writer.WriteByte(statusByte);

            //

            var contentSize = midiEvent.GetSize(settings);
            writer.WriteVlqNumber(contentSize);
            midiEvent.Write(writer, settings);
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
    }
}

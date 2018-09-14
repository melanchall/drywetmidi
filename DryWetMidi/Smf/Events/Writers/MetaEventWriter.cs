using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class MetaEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
                writer.WriteByte(EventStatusBytes.Global.Meta);

            //

            byte statusByte;

            var unknownMetaEvent = midiEvent as UnknownMetaEvent;
            if (unknownMetaEvent != null)
                statusByte = unknownMetaEvent.StatusByte;
            else
            {
                var eventType = midiEvent.GetType();
                if (!StandardEventTypes.Meta.TryGetStatusByte(eventType, out statusByte) && settings.CustomMetaEventTypes?.TryGetStatusByte(eventType, out statusByte) != true)
                    Debug.Fail($"Unable to write the {eventType} event.");
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

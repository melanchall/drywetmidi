using System;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class MetaEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (!(midiEvent is MetaEvent))
                throw new ArgumentException("Event is not Meta event.", nameof(midiEvent));

            //

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
                    throw new InvalidOperationException($"Unable to write the {eventType} event.");
            }

            writer.WriteByte(statusByte);

            //

            var contentSize = midiEvent.GetSize();
            writer.WriteVlqNumber(contentSize);
            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (!(midiEvent is MetaEvent))
                throw new ArgumentException("Event is not Meta event.", nameof(midiEvent));

            //

            var contentSize = midiEvent.GetSize();
            return (writeStatusByte ? 1 : 0) + 1 + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            return EventStatusBytes.Global.Meta;
        }

        #endregion
    }
}

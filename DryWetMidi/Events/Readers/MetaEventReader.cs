using System;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class MetaEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var statusByte = reader.ReadByte();
            var size = reader.ReadVlqNumber();

            //

            Type eventType;
            var midiEvent = TryGetEventType(settings.CustomMetaEventTypes, statusByte, out eventType)
                ? (MetaEvent)Activator.CreateInstance(eventType)
                : new UnknownMetaEvent(statusByte);

            //

            var readerPosition = reader.Position;
            midiEvent.Read(reader, settings, size);

            var bytesRead = reader.Position - readerPosition;
            var bytesUnread = size - bytesRead;
            if (bytesUnread > 0)
                reader.Position += bytesUnread;

            return midiEvent;
        }

        #endregion

        #region Methods

        private static bool TryGetEventType(EventTypesCollection customMetaEventTypes, byte statusByte, out Type eventType)
        {
            return StandardEventTypes.Meta.TryGetType(statusByte, out eventType) ||
                   (customMetaEventTypes?.TryGetType(statusByte, out eventType) == true && IsMetaEventType(eventType));
        }

        private static bool IsMetaEventType(Type type)
        {
            return type != null &&
                   type.IsSubclassOf(typeof(MetaEvent)) &&
                   type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}

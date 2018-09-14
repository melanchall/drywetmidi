using System;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class SystemCommonEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            Type eventType;
            StandardEventTypes.SystemCommon.TryGetType(currentStatusByte, out eventType);
            var systemCommonEvent = (SystemCommonEvent)Activator.CreateInstance(eventType);

            systemCommonEvent.Read(reader, settings, MidiEvent.UnknownContentSize);
            return systemCommonEvent;
        }

        #endregion
    }
}

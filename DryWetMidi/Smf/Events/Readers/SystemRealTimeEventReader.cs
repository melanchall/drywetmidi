using System;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class SystemRealTimeEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            Type eventType;
            StandardEventTypes.SystemRealTime.TryGetType(currentStatusByte, out eventType);
            var systemRealTimeEvent = (SystemRealTimeEvent)Activator.CreateInstance(eventType);

            systemRealTimeEvent.Read(reader, settings, MidiEvent.UnknownContentSize);
            return systemRealTimeEvent;
        }

        #endregion
    }
}

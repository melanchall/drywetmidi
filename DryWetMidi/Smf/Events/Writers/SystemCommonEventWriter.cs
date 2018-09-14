using System.Diagnostics;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class SystemCommonEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
            {
                var eventType = midiEvent.GetType();

                byte statusByte;
                if (!StandardEventTypes.SystemCommon.TryGetStatusByte(eventType, out statusByte))
                    Debug.Fail($"Unable to write the {eventType} event.");

                writer.WriteByte(statusByte);
            }

            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            var eventType = midiEvent.GetType();

            byte statusByte;
            if (!StandardEventTypes.SystemCommon.TryGetStatusByte(eventType, out statusByte))
                Debug.Fail($"No status byte defined for {eventType}.");

            return statusByte;
        }

        #endregion
    }
}

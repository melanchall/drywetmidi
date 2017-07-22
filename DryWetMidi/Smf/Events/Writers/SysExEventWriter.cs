using System.Diagnostics;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class SysExEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            VerifyEvent(midiEvent);

            //

            if (writeStatusByte)
            {
                var eventType = midiEvent.GetType();

                byte statusByte;
                if (!StandardEventTypes.SysEx.TryGetStatusByte(eventType, out statusByte))
                    Debug.Fail($"Unable to write the {eventType} event.");

                writer.WriteByte(statusByte);
            }

            //

            var contentSize = midiEvent.GetSize();
            writer.WriteVlqNumber(contentSize);
            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            VerifyEvent(midiEvent);

            //

            var contentSize = midiEvent.GetSize();
            return (writeStatusByte ? 1 : 0) + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            VerifyEvent(midiEvent);

            //

            byte statusByte;
            if (!StandardEventTypes.SysEx.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                Debug.Fail($"No status byte defined for {midiEvent.GetType()}.");

            return statusByte;
        }

        #endregion

        #region Methods

        [Conditional("DEBUG")]
        private static void VerifyEvent(MidiEvent midiEvent)
        {
            Debug.Assert(midiEvent != null);
            Debug.Assert(midiEvent is SysExEvent, "Event is not SysEx event.");
        }

        #endregion
    }
}

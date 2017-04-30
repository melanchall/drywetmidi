using System;

namespace Melanchall.DryWetMidi
{
    internal sealed class SysExEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            var sysExEvent = midiEvent as SysExEvent;
            if (sysExEvent == null)
                throw new ArgumentException("Event is not SysEx event.", nameof(midiEvent));

            //

            if (writeStatusByte)
            {
                byte statusByte;
                if (!StandardEventTypes.SysEx.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                    throw new Exception();

                writer.WriteByte(statusByte);
            }

            //

            var contentSize = midiEvent.GetContentSize();
            writer.WriteVlqNumber(contentSize);
            midiEvent.WriteContent(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (!(midiEvent is SysExEvent))
                throw new ArgumentException("Event is not SysEx event.", nameof(midiEvent));

            //

            var contentSize = midiEvent.GetContentSize();
            return (writeStatusByte ? 1 : 0) + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (!(midiEvent is SysExEvent))
                throw new ArgumentException("Event is not SysEx event.", nameof(midiEvent));

            //

            byte statusByte;
            if (!StandardEventTypes.SysEx.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                throw new Exception();

            return statusByte;
        }

        #endregion
    }
}

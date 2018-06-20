using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class ChannelEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
            {
                var eventType = midiEvent.GetType();

                byte statusByte;
                if (!StandardEventTypes.Channel.TryGetStatusByte(eventType, out statusByte))
                    Debug.Fail($"Unable to write the {eventType} event.");

                var channel = ((ChannelEvent)midiEvent).Channel;

                var totalStatusByte = DataTypesUtilities.Combine((FourBitNumber)statusByte, channel);
                writer.WriteByte(totalStatusByte);
            }

            //

            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            byte statusByte;
            if (!StandardEventTypes.Channel.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                Debug.Fail($"No status byte defined for {midiEvent.GetType()}.");

            return DataTypesUtilities.Combine((FourBitNumber)statusByte,
                                              ((ChannelEvent)midiEvent).Channel);
        }

        #endregion
    }
}

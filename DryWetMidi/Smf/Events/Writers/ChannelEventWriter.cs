using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class ChannelEventWriter : IEventWriter
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
            VerifyEvent(midiEvent);

            //

            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            VerifyEvent(midiEvent);

            //

            byte statusByte;
            if (!StandardEventTypes.Channel.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                Debug.Fail($"No status byte defined for {midiEvent.GetType()}.");

            return DataTypesUtilities.Combine((FourBitNumber)statusByte,
                                              ((ChannelEvent)midiEvent).Channel);
        }

        #endregion

        #region Methods

        [Conditional("DEBUG")]
        private static void VerifyEvent(MidiEvent midiEvent)
        {
            Debug.Assert(midiEvent != null);
            Debug.Assert(midiEvent is ChannelEvent, "Event is not Channel event.");
        }

        #endregion
    }
}

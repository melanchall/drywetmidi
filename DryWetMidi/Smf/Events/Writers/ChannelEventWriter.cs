using System;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class ChannelEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            var channelEvent = midiEvent as ChannelEvent;
            if (channelEvent == null)
                throw new ArgumentException("Event is not Channel event.", nameof(midiEvent));

            //

            if (writeStatusByte)
            {
                var eventType = midiEvent.GetType();

                byte statusByte;
                if (!StandardEventTypes.Channel.TryGetStatusByte(eventType, out statusByte))
                    throw new InvalidOperationException($"Unable to write the {eventType} event.");

                var channel = channelEvent.Channel;

                var totalStatusByte = DataTypesUtilities.Combine((FourBitNumber)statusByte, channel);
                writer.WriteByte(totalStatusByte);
            }

            //

            midiEvent.Write(writer, settings);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (!(midiEvent is ChannelEvent))
                throw new ArgumentException("Event is not Channel event.", nameof(midiEvent));

            //

            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize();
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            var channelEvent = midiEvent as ChannelEvent;
            if (channelEvent == null)
                throw new ArgumentException("Event is not Channel event.", nameof(midiEvent));

            //

            byte statusByte;
            if (!StandardEventTypes.Channel.TryGetStatusByte(midiEvent.GetType(), out statusByte))
                Debug.Fail($"No status byte defined for {midiEvent.GetType()}.");

            var channel = channelEvent.Channel;

            return DataTypesUtilities.Combine((FourBitNumber)statusByte, channel);
        }

        #endregion
    }
}

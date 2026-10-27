using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemRealTimeEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            if (writeStatusByte)
            {
                var statusByte = GetStatusByte(midiEvent);
                writer.WriteByte(statusByte);
            }

            midiEvent.Write(writer, settings);
        }

        public async Task WriteAsync(
            MidiEvent midiEvent,
            MidiWriter writer,
            WritingSettings settings,
            bool writeStatusByte,
            CancellationToken cancellationToken)
        {
            if (writeStatusByte)
            {
                var statusByte = GetStatusByte(midiEvent);
                await writer.WriteByteAsync(statusByte, cancellationToken).ConfigureAwait(false);
            }

            await midiEvent.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            return (writeStatusByte ? 1 : 0) + midiEvent.GetSize(settings);
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            switch (midiEvent.EventType)
            {
                case MidiEventType.ActiveSensing:
                    return EventStatusBytes.SystemRealTime.ActiveSensing;
                case MidiEventType.Continue:
                    return EventStatusBytes.SystemRealTime.Continue;
                case MidiEventType.Reset:
                    return EventStatusBytes.SystemRealTime.Reset;
                case MidiEventType.Start:
                    return EventStatusBytes.SystemRealTime.Start;
                case MidiEventType.Stop:
                    return EventStatusBytes.SystemRealTime.Stop;
                case MidiEventType.TimingClock:
                    return EventStatusBytes.SystemRealTime.TimingClock;
            }

            return 0;
        }

        #endregion
    }
}

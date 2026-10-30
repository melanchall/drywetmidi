using Melanchall.DryWetMidi.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SysExEventWriter : IEventWriter
    {
        #region IEventWriter

        public void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte)
        {
            var statusByte = GetStatusByte(midiEvent);
            writer.WriteByte(statusByte);

            var contentSize = midiEvent.GetSize(settings);
            writer.WriteVlqNumber(contentSize);
            midiEvent.Write(writer, settings);
        }

        public async Task WriteAsync(
            MidiEvent midiEvent,
            MidiWriter writer,
            WritingSettings settings,
            bool writeStatusByte,
            CancellationToken cancellationToken)
        {
            var statusByte = GetStatusByte(midiEvent);
            await writer.WriteByteAsync(statusByte, cancellationToken).ConfigureAwait(false);

            var contentSize = midiEvent.GetSize(settings);
            await writer.WriteVlqNumberAsync(contentSize, cancellationToken).ConfigureAwait(false);
            await midiEvent.WriteAsync(writer, settings, cancellationToken).ConfigureAwait(false);
        }

        public int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte)
        {
            var contentSize = midiEvent.GetSize(settings);
            return 1 + contentSize.GetVlqLength() + contentSize;
        }

        public byte GetStatusByte(MidiEvent midiEvent)
        {
            switch (midiEvent.EventType)
            {
                case MidiEventType.NormalSysEx:
                    return EventStatusBytes.Global.NormalSysEx;
                case MidiEventType.EscapeSysEx:
                    return EventStatusBytes.Global.EscapeSysEx;
            }

            return 0;
        }

        #endregion
    }
}

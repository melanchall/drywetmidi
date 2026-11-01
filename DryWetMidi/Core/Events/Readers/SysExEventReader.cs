using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SysExEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var size = reader.ReadVlqNumber();

            var sysExEvent = CreateEvent(currentStatusByte);
            sysExEvent.Read(reader, settings, size);
            return sysExEvent;
        }

        public async Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken)
        {
            var size = await reader.ReadVlqNumberAsync(cancellationToken).ConfigureAwait(false);

            var sysExEvent = CreateEvent(currentStatusByte);
            await sysExEvent.ReadAsync(reader, settings, size, cancellationToken).ConfigureAwait(false);
            return sysExEvent;
        }

        #endregion

        #region Methods

        private static SysExEvent CreateEvent(byte currentStatusByte)
        {
            switch (currentStatusByte)
            {
                case EventStatusBytes.Global.NormalSysEx:
                    return new NormalSysExEvent();
                case EventStatusBytes.Global.EscapeSysEx:
                    return new EscapeSysExEvent();
                default:
                    // TODO: proper exception
                    throw new InvalidOperationException($"Unexpected status byte: {currentStatusByte}");
            }
        }

        #endregion
    }
}

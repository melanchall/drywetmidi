using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemCommonEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var systemCommonEvent = CreateEvent(currentStatusByte);
            systemCommonEvent?.Read(reader, settings, MidiEvent.UnknownContentSize);
            return systemCommonEvent;
        }

        public async Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken)
        {
            var systemCommonEvent = CreateEvent(currentStatusByte);
            if (systemCommonEvent != null)
                await systemCommonEvent.ReadAsync(reader, settings, MidiEvent.UnknownContentSize, cancellationToken).ConfigureAwait(false);

            return systemCommonEvent;
        }

        #endregion

        #region Methods

        private static SystemCommonEvent? CreateEvent(byte currentStatusByte)
        {
            switch (currentStatusByte)
            {
                case EventStatusBytes.SystemCommon.MtcQuarterFrame:
                    return new MidiTimeCodeEvent();
                case EventStatusBytes.SystemCommon.SongSelect:
                    return new SongSelectEvent();
                case EventStatusBytes.SystemCommon.SongPositionPointer:
                    return new SongPositionPointerEvent();
                case EventStatusBytes.SystemCommon.TuneRequest:
                    return new TuneRequestEvent();
            }

            return null;
        }

        #endregion
    }
}

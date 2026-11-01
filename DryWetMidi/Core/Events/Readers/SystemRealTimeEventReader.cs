using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemRealTimeEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var systemRealTimeEvent = CreateEvent(currentStatusByte);
            systemRealTimeEvent?.Read(reader, settings, MidiEvent.UnknownContentSize);
            return systemRealTimeEvent;
        }

        public async Task<MidiEvent?> ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            byte currentStatusByte,
            CancellationToken cancellationToken)
        {
            var systemRealTimeEvent = CreateEvent(currentStatusByte);
            if (systemRealTimeEvent != null)
                await systemRealTimeEvent.ReadAsync(reader, settings, MidiEvent.UnknownContentSize, cancellationToken).ConfigureAwait(false);

            return systemRealTimeEvent;
        }

        #endregion

        #region Methods

        private static SystemRealTimeEvent? CreateEvent(byte currentStatusByte)
        {
            switch (currentStatusByte)
            {
                case EventStatusBytes.SystemRealTime.ActiveSensing:
                    return new ActiveSensingEvent();
                case EventStatusBytes.SystemRealTime.Continue:
                    return new ContinueEvent();
                case EventStatusBytes.SystemRealTime.Reset:
                    return new ResetEvent();
                case EventStatusBytes.SystemRealTime.Start:
                    return new StartEvent();
                case EventStatusBytes.SystemRealTime.Stop:
                    return new StopEvent();
                case EventStatusBytes.SystemRealTime.TimingClock:
                    return new TimingClockEvent();
            }

            return null;
        }

        #endregion
    }
}

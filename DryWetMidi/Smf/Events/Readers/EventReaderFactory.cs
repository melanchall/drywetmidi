using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provides a way to get <see cref="IEventReader"/> for an event.
    /// </summary>
    internal static class EventReaderFactory
    {
        #region Fields

        private static readonly IEventReader _metaEventReader = new MetaEventReader();
        private static readonly IEventReader _channelEventReader = new ChannelEventReader();
        private static readonly IEventReader _sysExEventReader = new SysExEventReader();
        private static readonly IEventReader _systemRealTimeEventReader = new SystemRealTimeEventReader();
        private static readonly IEventReader _systemCommonEventReader = new SystemCommonEventReader();

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="IEventReader"/> for an event with the specified status byte.
        /// </summary>
        /// <param name="statusByte">Status byte to get reader for.</param>
        /// <param name="smfOnly">Indicates whether only reader for SMF events should be returned or not.</param>
        /// <returns>Reader for an event with the specified status byte.</returns>
        internal static IEventReader GetReader(byte statusByte, bool smfOnly)
        {
            if (statusByte == EventStatusBytes.Global.Meta)
                return _metaEventReader;

            if (statusByte == EventStatusBytes.Global.EscapeSysEx ||
                statusByte == EventStatusBytes.Global.NormalSysEx)
                return _sysExEventReader;

            if (!smfOnly)
            {
                if (EventStatusBytes.SystemRealTime.StatusBytes.Contains(statusByte))
                    return _systemRealTimeEventReader;

                if (EventStatusBytes.SystemCommon.StatusBytes.Contains(statusByte))
                    return _systemCommonEventReader;
            }

            return _channelEventReader;
        }

        #endregion
    }
}

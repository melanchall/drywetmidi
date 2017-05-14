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

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="IEventReader"/> for an event with the specified status byte.
        /// </summary>
        /// <param name="statusByte">Status byte to get reader for.</param>
        /// <returns>Reader for an event with the specified status byte.</returns>
        internal static IEventReader GetReader(byte statusByte)
        {
            if (statusByte == EventStatusBytes.Global.Meta)
                return _metaEventReader;

            if (statusByte == EventStatusBytes.Global.EscapeSysEx || statusByte == EventStatusBytes.Global.NormalSysEx)
                return _sysExEventReader;

            return _channelEventReader;
        }

        #endregion
    }
}

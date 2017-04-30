namespace Melanchall.DryWetMidi
{
    public static class EventReaderFactory
    {
        #region Fields

        private static readonly IEventReader _metaEventReader = new MetaEventReader();
        private static readonly IEventReader _channelEventReader = new ChannelEventReader();
        private static readonly IEventReader _sysExEventReader = new SysExEventReader();

        #endregion

        #region Methods

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

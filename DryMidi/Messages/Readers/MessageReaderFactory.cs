namespace Melanchall.DryMidi
{
    public static class MessageReaderFactory
    {
        #region Fields

        private static readonly IMessageReader _metaMessageReader = new MetaMessageReader();
        private static readonly IMessageReader _channelMessageReader = new ChannelMessageReader();
        private static readonly IMessageReader _sysExMessageReader = new SysExMessageReader();

        #endregion

        #region Methods

        public static IMessageReader GetReader(byte statusByte)
        {
            if (statusByte == MessagesStatusBytes.Global.Meta)
                return _metaMessageReader;

            if (statusByte == MessagesStatusBytes.Global.EscapeSysEx || statusByte == MessagesStatusBytes.Global.NormalSysEx)
                return _sysExMessageReader;

            return _channelMessageReader;
        }

        #endregion
    }
}

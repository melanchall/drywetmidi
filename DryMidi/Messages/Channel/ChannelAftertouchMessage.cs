namespace Melanchall.DryMidi
{
    public sealed class ChannelAftertouchMessage : ChannelMessage
    {
        #region Constructor

        public ChannelAftertouchMessage()
            : base(1)
        {
        }

        public ChannelAftertouchMessage(SevenBitNumber aftertouchValue)
            : this()
        {
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber AftertouchValue
        {
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Channel Aftertouch (channel = {Channel}, aftertouch value = {AftertouchValue})";
        }

        #endregion
    }
}

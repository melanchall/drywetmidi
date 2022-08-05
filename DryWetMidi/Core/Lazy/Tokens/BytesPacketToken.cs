namespace Melanchall.DryWetMidi.Core
{
    public sealed class BytesPacketToken : MidiToken
    {
        #region Constructor

        internal BytesPacketToken(byte[] data)
            : base(MidiTokenType.BytesPacket)
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Bytes packet token ({Data.Length} byte(s))";
        }

        #endregion
    }
}

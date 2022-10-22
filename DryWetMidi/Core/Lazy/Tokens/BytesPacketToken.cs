namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a bytes packet.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
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

        /// <summary>
        /// Gets the data of the current bytes packet.
        /// </summary>
        public byte[] Data { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Bytes packet token ({Data.Length} byte(s))";
        }

        #endregion
    }
}

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class AckSysExData : HandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "ACK";
        }

        #endregion
    }
}

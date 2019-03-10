namespace Melanchall.DryWetMidi.Smf
{
    public sealed class NakSysExData : HandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "NAK";
        }

        #endregion
    }
}

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class EofSysExData : HandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "EOF";
        }

        #endregion
    }
}

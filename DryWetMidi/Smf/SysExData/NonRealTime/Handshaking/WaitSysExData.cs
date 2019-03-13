namespace Melanchall.DryWetMidi.Smf
{
    public sealed class WaitSysExData : HandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "WAIT";
        }

        #endregion
    }
}

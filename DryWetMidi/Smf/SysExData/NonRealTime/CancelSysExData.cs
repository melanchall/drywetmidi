namespace Melanchall.DryWetMidi.Smf
{
    public sealed class CancelSysExData : HandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "CANCEL";
        }

        #endregion
    }
}

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class NakSysExData : SampleDumpHandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "NAK";
        }

        #endregion
    }
}

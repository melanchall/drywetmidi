namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SampleDumpCancelSysExData : SampleDumpHandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "CANCEL";
        }

        #endregion
    }
}

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class SampleDumpWaitSysExData : SampleDumpHandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "WAIT";
        }

        #endregion
    }
}

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class AckSysExData : SampleDumpHandshakingSysExData
    {
        #region Overrides

        public override string ToString()
        {
            return "ACK";
        }

        #endregion
    }
}

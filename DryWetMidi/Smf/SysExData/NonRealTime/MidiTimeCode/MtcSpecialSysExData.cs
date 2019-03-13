namespace Melanchall.DryWetMidi.Smf
{
    public sealed class MtcSpecialSysExData : MtcSysExData
    {
        #region Properties

        public MtcSpecialType Type => (MtcSpecialType)EventNumber;

        #endregion

        #region Overrides

        public override string ToString()
        {
            return "MTC Special";
        }

        #endregion
    }
}

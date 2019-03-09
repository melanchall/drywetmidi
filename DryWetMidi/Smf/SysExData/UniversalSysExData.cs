using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public abstract class UniversalSysExData : SysExData
    {
        #region Properties

        public SevenBitNumber Channel { get; set; }

        #endregion
    }
}

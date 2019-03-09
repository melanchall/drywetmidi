namespace Melanchall.DryWetMidi.Smf
{
    public sealed class UnknownSysExData : SysExData
    {
        #region Constructor

        internal UnknownSysExData(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
        }

        public override string ToString()
        {
            return "Unknown sys ex data";
        }

        #endregion
    }
}

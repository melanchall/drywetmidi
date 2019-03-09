namespace Melanchall.DryWetMidi.Smf
{
    internal static class SysExDataReaderFactory
    {
        #region Fields

        private static readonly ISysExDataReader _nonRealTimeSysExDataReader = new NonRealTimeSysExDataReader();
        private static readonly ISysExDataReader _realTimeSysExDataReader = new RealTimeSysExDataReader();

        #endregion

        #region Methods

        public static ISysExDataReader GetReader(byte[] data)
        {
            var statusByte = data[0];

            if (statusByte == UniversalSysExDataStatusBytes.NonRealTime)
                return _nonRealTimeSysExDataReader;

            if (statusByte == UniversalSysExDataStatusBytes.RealTime)
                return _realTimeSysExDataReader;

            return null;
        }

        #endregion
    }
}

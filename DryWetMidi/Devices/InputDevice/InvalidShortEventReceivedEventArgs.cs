namespace Melanchall.DryWetMidi.Devices
{
    public sealed class InvalidShortEventReceivedEventArgs
    {
        #region Constructor

        internal InvalidShortEventReceivedEventArgs(byte statusByte, byte firstDataByte, byte secondDataByte)
        {
            StatusByte = statusByte;
            FirstDataByte = firstDataByte;
            SecondDataByte = secondDataByte;
        }

        #endregion

        #region Properties

        public byte StatusByte { get; }

        public byte FirstDataByte { get; }

        public byte SecondDataByte { get; }

        #endregion
    }
}

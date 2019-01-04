namespace Melanchall.DryWetMidi.Devices
{
    public sealed class InvalidSysExEventReceivedEventArgs
    {
        #region Constructor

        internal InvalidSysExEventReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; }

        #endregion
    }
}

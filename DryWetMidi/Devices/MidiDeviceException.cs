using System;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiDeviceException : Exception
    {
        #region Constructor

        public MidiDeviceException()
            : base()
        {
        }

        public MidiDeviceException(string message)
            : base(message)
        {
        }

        public MidiDeviceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class TickGeneratorException : MidiException
    {
        #region Constructor

        public TickGeneratorException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        #endregion

        #region Properties

        public int ErrorCode { get; }

        #endregion
    }
}

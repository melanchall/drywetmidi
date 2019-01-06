using System;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class ErrorOccurredEventArgs
    {
        #region Constructor

        internal ErrorOccurredEventArgs(Exception exception)
        {
            Exception = exception;
        }

        #endregion

        #region Properties

        public Exception Exception { get; }

        #endregion
    }
}

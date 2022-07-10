using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for an event indicating an error occurred on a device.
    /// </summary>
    public sealed class ErrorOccurredEventArgs : EventArgs
    {
        #region Constructor

        internal ErrorOccurredEventArgs(Exception exception)
        {
            Exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the exception caused an error.
        /// </summary>
        public Exception Exception { get; }

        #endregion
    }
}

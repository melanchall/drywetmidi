using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for the <see cref="MidiDevice.ErrorOccurred"/> event.
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

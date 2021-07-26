using System;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Base MIDI exception class.
    /// </summary>
    public abstract class MidiException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiException"/>.
        /// </summary>
        internal MidiException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        internal MidiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiException"/> class with the
        /// specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a <c>null</c> reference if no inner exception is specified.</param>
        internal MidiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}

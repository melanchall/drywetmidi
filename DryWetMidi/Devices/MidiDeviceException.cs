using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// The exception that is thrown when an error occurred on a MIDI device.
    /// </summary>
    public sealed class MidiDeviceException : Exception
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiDeviceException"/>.
        /// </summary>
        public MidiDeviceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiDeviceException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public MidiDeviceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiDeviceException"/> class with the
        /// specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a <c>null</c> reference if no inner exception is specified.</param>
        public MidiDeviceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}

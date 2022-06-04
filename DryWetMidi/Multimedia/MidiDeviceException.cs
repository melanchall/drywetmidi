using System;
using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// The exception that is thrown when an error occurred on a MIDI device.
    /// </summary>
    [Serializable]
    public sealed class MidiDeviceException : MidiException
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiDeviceException"/> class with the
        /// specified error message and an error code.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">The error code.</param>
        public MidiDeviceException(string message, int errorCode)
            : this(message)
        {
            ErrorCode = errorCode;
        }

        private MidiDeviceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = (int?)info.GetValue(nameof(ErrorCode), typeof(int?));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the code of an error represented by the current <see cref="MidiDeviceException"/>.
        /// </summary>
        public int? ErrorCode { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ErrorCode), ErrorCode);
        }

        #endregion
    }
}

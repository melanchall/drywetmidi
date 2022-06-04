using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// The exception that is thrown when an error occurred on <see cref="TickGenerator"/>.
    /// </summary>
    [Serializable]
    public sealed class TickGeneratorException : MidiException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TickGeneratorException"/> class with the
        /// specified error message and an error code.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">The error code.</param>
        public TickGeneratorException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        private TickGeneratorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetInt32(nameof(ErrorCode));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the code of an error represented by the current <see cref="MidiDeviceException"/>.
        /// </summary>
        public int ErrorCode { get; }

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

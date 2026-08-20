using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// The exception that is thrown when an error occurred on <see cref="TickGenerator"/>.
    /// </summary>
    public sealed class TickGeneratorException : MidiException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TickGeneratorException"/> class with the
        /// specified error message and an error code.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="mainErrorCode">The error code.</param>
        public TickGeneratorException(
            string message,
            int mainErrorCode,
            int additionalErrorCode)
            : base(message)
        {
            MainErrorCode = mainErrorCode;
            AdditionalErrorCode = additionalErrorCode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the code of an error represented by the current <see cref="TickGeneratorException"/>.
        /// </summary>
        public int MainErrorCode { get; }

        public int AdditionalErrorCode { get; }

        #endregion
    }
}

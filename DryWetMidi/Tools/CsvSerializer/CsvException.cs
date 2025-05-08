using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Represents an exception that occurs during CSV deserialization.
    /// </summary>
    public sealed class CsvException : MidiException
    {
        #region Constructors

        internal CsvException(
            string message,
            int? lineNumber,
            Exception innerException)
            : base(message, innerException)
        {
            LineNumber = lineNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of a line in source CSV where an error occurred.
        /// If the error is not related to a specific line, this property will return <c>null</c>.
        /// </summary>
        public int? LineNumber { get; private set; }

        #endregion
    }
}

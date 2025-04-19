using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Tools
{
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

        public int? LineNumber { get; private set; }

        #endregion
    }
}

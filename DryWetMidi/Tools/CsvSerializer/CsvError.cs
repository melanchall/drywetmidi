using System;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvError
    {
        #region Methods

        public static CsvException BadFormat(int? lineNumber, string message, Exception? innerException = null)
        {
            return new CsvException(
                $"{(lineNumber != null ? $"Line {lineNumber}: " : string.Empty)}{message}",
                lineNumber,
                innerException);
        }

        public static CsvException BadFormat(string message, Exception? innerException = null)
        {
            return BadFormat(null, message, innerException);
        }

        #endregion
    }
}

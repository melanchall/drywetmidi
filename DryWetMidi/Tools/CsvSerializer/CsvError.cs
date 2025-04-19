using System;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvError
    {
        #region Methods

        public static void ThrowBadFormat(int? lineNumber, string message, Exception innerException = null)
        {
            throw new CsvException(
                $"{(lineNumber != null ? $"Line {lineNumber}: " : string.Empty)}{message}",
                lineNumber,
                innerException);
        }

        public static void ThrowBadFormat(string message, Exception innerException = null)
        {
            ThrowBadFormat(null, message, innerException);
        }

        #endregion
    }
}

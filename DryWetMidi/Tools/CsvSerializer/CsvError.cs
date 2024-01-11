using System;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvError
    {
        #region Methods

        public static void ThrowBadFormat(int lineNumber, string message, Exception innerException = null)
        {
            ThrowBadFormat($"Line {lineNumber}: {message}", innerException);
        }

        public static void ThrowBadFormat(string message, Exception innerException = null)
        {
            throw new FormatException(message, innerException);
        }

        #endregion
    }
}

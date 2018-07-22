namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvUtilities
    {
        #region Constants

        private const char Quote = '"';
        private const string QuoteString = "\"";
        private const string DoubleQuote = "\"\"";

        #endregion

        #region Methods

        public static string EscapeString(string input)
        {
            return $"{Quote}{input.Replace(QuoteString, DoubleQuote)}{Quote}";
        }

        public static string UnescapeString(string input)
        {
            return input.Replace(DoubleQuote, QuoteString).Trim(Quote);
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvFormattingUtilities
    {
        #region Constants

        private const char Quote = '"';
        private const string QuoteString = "\"";
        private const string DoubleQuote = "\"\"";

        #endregion

        #region Methods

        public static object FormatTime(
            ITimedObject obj,
            TimeSpanType timeType,
            TempoMap tempoMap)
        {
            return obj.TimeAs(timeType, tempoMap);
        }

        public static object FormatLength(
            ILengthedObject obj,
            TimeSpanType lengthType,
            TempoMap tempoMap)
        {
            return obj.LengthAs(lengthType, tempoMap);
        }

        public static object FormatNoteNumber(SevenBitNumber noteNumber, CsvNoteFormat noteNumberFormat)
        {
            switch (noteNumberFormat)
            {
                case CsvNoteFormat.Letter:
                    return MusicTheory.Note.Get(noteNumber);
            }

            return noteNumber;
        }

        public static string EscapeString(string input)
        {
            return $"{Quote}{input.Replace(QuoteString, DoubleQuote)}{Quote}";
        }

        public static string UnescapeString(string input)
        {
            if (input.Length > 1 && input[0] == '\"' && input[input.Length - 1] == '\"')
                input = input.Substring(1, input.Length - 2);

            return input.Replace(DoubleQuote, QuoteString);
        }

        #endregion
    }
}

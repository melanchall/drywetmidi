namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvRecord
    {
        #region Constructor

        public CsvRecord(int lineNumber, int linesCount, string[] values)
        {
            LineNumber = lineNumber;
            LinesCount = linesCount;
            Values = values;
        }

        #endregion

        #region Properties

        public int LineNumber { get; }

        public int LinesCount { get; }

        public string[] Values { get; }

        #endregion
    }
}

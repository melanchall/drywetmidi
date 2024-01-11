using System.Collections.Generic;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvWriter2
    {
        #region Fields

        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly string _delimiter;

        #endregion

        #region Constructor

        public CsvWriter2(string delimiter)
        {
            _delimiter = delimiter;
        }

        #endregion

        #region Methods

        public void WriteRecord(IEnumerable<object> values)
        {
            _stringBuilder.AppendLine(string.Join(_delimiter.ToString(), values));
        }

        public void WriteRecord(params object[] values)
        {
            WriteRecord((IEnumerable<object>)values);
        }

        public string GetCsv()
        {
            return _stringBuilder.ToString().Trim();
        }

        #endregion
    }
}

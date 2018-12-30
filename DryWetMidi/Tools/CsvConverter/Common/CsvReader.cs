using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvReader : IDisposable
    {
        #region Constants

        private const char Quote = '"';
        private const string QuoteString = "\"";
        private const string DoubleQuote = "\"\"";

        #endregion

        #region Fields

        private readonly StreamReader _streamReader;
        private readonly char _delimiter;

        private bool _disposed = false;
        private int _currentLineNumber = 0;

        #endregion

        #region Constructor

        public CsvReader(Stream stream, char delimiter)
        {
            _streamReader = new StreamReader(stream);
            _delimiter = delimiter;
        }

        #endregion

        #region Methods

        public CsvRecord ReadRecord()
        {
            var oldLineNumber = _currentLineNumber;

            var line = GetNextLine();
            if (string.IsNullOrEmpty(line))
                return null;

            string[] values;

            while (true)
            {
                values = SplitValues(line, _delimiter).ToArray();
                if (values.All(IsValueClosed))
                    break;

                var nextLine = GetNextLine();
                if (nextLine == null)
                    break;

                line = line + Environment.NewLine + nextLine;
            }

            return new CsvRecord(oldLineNumber, _currentLineNumber - oldLineNumber, values);
        }

        private string GetNextLine()
        {
            var result = string.Empty;

            do
            {
                result = _streamReader.ReadLine();
                _currentLineNumber++;
            }
            while (result == string.Empty);

            return result;
        }

        private static IEnumerable<string> SplitValues(string input, char delimiter)
        {
            var valueBuilder = new StringBuilder();
            var escapedString = false;
            var possibleFinishedValue = false;

            foreach (var c in input)
            {
                if (c == delimiter && (!escapedString || possibleFinishedValue))
                {
                    yield return valueBuilder.ToString().Trim();

                    valueBuilder.Clear();
                    possibleFinishedValue = false;
                    escapedString = false;
                    continue;
                }

                if (c == Quote)
                {
                    if (!escapedString)
                        escapedString = true;
                    else
                        possibleFinishedValue = !possibleFinishedValue;
                }

                valueBuilder.Append(c);
            }

            yield return valueBuilder.ToString().Trim();
        }

        private static bool IsValueClosed(string value)
        {
            if (string.IsNullOrEmpty(value) || value[0] != Quote)
                return true;

            if (value.Length == 1)
                return false;

            return value.Skip(1).Reverse().TakeWhile(c => c == Quote).Count() % 2 == 1;
        }

        #endregion

        #region IDisposable

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _streamReader.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}

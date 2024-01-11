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

        #endregion

        #region Fields

        private readonly StreamReader _streamReader;
        private readonly char _delimiter;

        private readonly char[] _buffer;
        private int _bufferLength = 0;
        private int _indexInBuffer = 0;

        private bool _disposed = false;
        private int _currentLineNumber = 0;

        #endregion

        #region Constructor

        public CsvReader(Stream stream, CsvSerializationSettings settings)
        {
            _streamReader = new StreamReader(stream, Encoding.UTF8, true, settings.ReadWriteBufferSize, true);
            _buffer = new char[settings.ReadWriteBufferSize];
            _delimiter = settings.Delimiter;
        }

        #endregion

        #region Methods

        public CsvRecord ReadRecord()
        {
            var oldLineNumber = _currentLineNumber;

            var line = GetFirstLine();
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

                line += nextLine;
            }

            return new CsvRecord(oldLineNumber, _currentLineNumber - oldLineNumber, values.Select(v => CsvFormattingUtilities.UnescapeString(v)).ToArray());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private string GetFirstLine()
        {
            string result;

            do
            {
                result = GetNextLine();
            }
            while (result?.Trim() == string.Empty);

            return result;
        }

        private string GetNextLine()
        {
            _currentLineNumber++;

            var stringBuilder = new StringBuilder();
            var lineEnding = false;

            while (true)
            {
                for (; _indexInBuffer < _bufferLength; _indexInBuffer++)
                {
                    var c = _buffer[_indexInBuffer];
                    if (c == '\r' || c == '\n')
                    {
                        lineEnding = true;
                    }
                    else if (lineEnding)
                        break;

                    stringBuilder.Append(c);
                }

                if (_indexInBuffer >= _bufferLength)
                    FillBuffer();
                else
                    break;

                if (_bufferLength == 0)
                    break;
            }

            return stringBuilder.Length > 0
                ? stringBuilder.ToString()
                : null;
        }

        private void FillBuffer()
        {
            var readCharsCount = 0;
            var unreadCharsCount = _buffer.Length;

            while (unreadCharsCount > 0)
            {
                var count = _streamReader.ReadBlock(_buffer, readCharsCount, unreadCharsCount);
                if (count == 0)
                    break;

                unreadCharsCount -= count;
                readCharsCount += count;
            }

            _bufferLength = _buffer.Length - unreadCharsCount;
            _indexInBuffer = 0;
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

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _streamReader.Dispose();

            _disposed = true;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvReader : IDisposable
    {
        private const char Quote = '"';

        private readonly StreamReader _streamReader;
        private readonly char _delimiter;
        private readonly char[] _buffer;
        private int _bufferLength = 0;
        private int _indexInBuffer = 0;
        private bool _disposed = false;
        private int _currentLineNumber = 0;

        private readonly List<string> _recordValuesBuilder = new();

        public CsvReader(Stream stream, CsvDeserializationSettings settings)
        {
            _streamReader = new StreamReader(stream, Encoding.UTF8, true, settings.BufferSize, true);
            _buffer = new char[settings.BufferSize];
            _delimiter = settings.Delimiter;
        }

        public CsvRecord? ReadRecord()
        {
            var line = GetFirstLine();
            var lineNumber = _currentLineNumber - 1;
            if (line == null)
                return null;

            _recordValuesBuilder.Clear();

            while (true)
            {
                SplitValues(line, _delimiter, _recordValuesBuilder);

                if (AreAllValuesClosed(_recordValuesBuilder))
                    break;

                var nextLine = GetNextLine();
                if (nextLine == null)
                    break;

                line += nextLine;

                _recordValuesBuilder.Clear();
            }

            var finalValues = new string[_recordValuesBuilder.Count];
            
            for (int i = 0; i < _recordValuesBuilder.Count; i++)
            {
                finalValues[i] = CsvFormattingUtilities.UnescapeString(_recordValuesBuilder[i]);
            }

            return new CsvRecord(lineNumber, _currentLineNumber - lineNumber, finalValues);
        }

        private string? GetFirstLine()
        {
            string? result;

            do
            {
                result = GetNextLine();
            }
            while (result != null && result.AsSpan().Trim().IsEmpty);

            return result;
        }

        private string? GetNextLine()
        {
            _currentLineNumber++;
            var stringBuilder = new StringBuilder();
            var lineEnding = false;

            while (true)
            {
                var start = _indexInBuffer;

                for (; _indexInBuffer < _bufferLength && !lineEnding; _indexInBuffer++)
                {
                    if (_buffer[_indexInBuffer] == '\n')
                        lineEnding = true;
                }

                if (_indexInBuffer > start)
                {
                    stringBuilder.Append(new ReadOnlySpan<char>(_buffer, start, _indexInBuffer - start));
                }

                if (_indexInBuffer >= _bufferLength)
                    FillBuffer();
                else
                    break;

                if (_bufferLength == 0)
                    break;
            }

            return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
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

        private static void SplitValues(string input, char delimiter, List<string> destination)
        {
            var span = input.AsSpan();

            var escapedString = false;
            var possibleFinishedValue = false;
            int startIdx = 0;

            for (int i = 0; i < span.Length; i++)
            {
                var c = span[i];

                if (c == delimiter && (!escapedString || possibleFinishedValue))
                {
                    var valueSpan = span[startIdx..i].Trim();
                    destination.Add(valueSpan.ToString());

                    possibleFinishedValue = false;
                    escapedString = false;
                    startIdx = i + 1;

                    continue;
                }

                if (c == Quote)
                {
                    if (!escapedString)
                        escapedString = true;
                    else
                        possibleFinishedValue = !possibleFinishedValue;
                }
            }

            var lastSpan = span[startIdx..].Trim();
            destination.Add(lastSpan.ToString());
        }

        private static bool AreAllValuesClosed(List<string> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (!IsValueClosed(values[i].AsSpan()))
                    return false;
            }

            return true;
        }

        private static bool IsValueClosed(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty || value[0] != Quote)
                return true;

            if (value.Length == 1)
                return false;

            var quoteCount = 0;

            for (var i = value.Length - 1; i >= 1; i--)
            {
                if (value[i] == Quote)
                    quoteCount++;
                else
                    break;
            }

            return quoteCount % 2 == 1;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _streamReader.Dispose();
                _disposed = true;
            }
        }
    }
}

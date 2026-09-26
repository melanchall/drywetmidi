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
            StringBuilder? recordBuilder = null;

            while (true)
            {
                if (SplitValues(line, _delimiter, _recordValuesBuilder))
                    break;

                recordBuilder ??= new StringBuilder(line);

                var nextLine = GetNextLine();
                if (nextLine == null)
                    break;

                recordBuilder.Append(nextLine);
                line = recordBuilder.ToString();

                _recordValuesBuilder.Clear();
            }

            return new CsvRecord(lineNumber, _currentLineNumber - lineNumber, _recordValuesBuilder.ToArray());
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
            StringBuilder? stringBuilder = null;

            while (true)
            {
                if (_indexInBuffer >= _bufferLength)
                    FillBuffer();

                if (_bufferLength == 0)
                    break;

                var start = _indexInBuffer;

                for (; _indexInBuffer < _bufferLength; _indexInBuffer++)
                {
                    if (_buffer[_indexInBuffer] == '\n')
                    {
                        _indexInBuffer++;

                        if (stringBuilder == null)
                            return new string(_buffer, start, _indexInBuffer - start);

                        stringBuilder.Append(new ReadOnlySpan<char>(_buffer, start, _indexInBuffer - start));
                        return stringBuilder.ToString();
                    }
                }

                if (_indexInBuffer > start)
                {
                    stringBuilder ??= new StringBuilder();
                    stringBuilder.Append(new ReadOnlySpan<char>(_buffer, start, _indexInBuffer - start));
                }

                FillBuffer();
            }

            return stringBuilder?.Length > 0 ? stringBuilder.ToString() : null;
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

        private static bool SplitValues(string input, char delimiter, List<string> destination)
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
                    destination.Add(CreateValue(input, startIdx, i));

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

            destination.Add(CreateValue(input, startIdx, input.Length));
            return IsValueClosed(span[startIdx..].Trim());
        }

        private static string CreateValue(string input, int startIndex, int endIndex)
        {
            while (startIndex < endIndex && char.IsWhiteSpace(input[startIndex]))
                startIndex++;

            while (endIndex > startIndex && char.IsWhiteSpace(input[endIndex - 1]))
                endIndex--;

            if (endIndex - startIndex <= 1 || input[startIndex] != Quote || input[endIndex - 1] != Quote)
                return input.AsSpan(startIndex, endIndex - startIndex).ToString();

            startIndex++;
            endIndex--;

            var escapedQuotesCount = 0;
            for (var i = startIndex; i < endIndex; i++)
            {
                if (input[i] == Quote && i + 1 < endIndex && input[i + 1] == Quote)
                {
                    escapedQuotesCount++;
                    i++;
                }
            }

            if (escapedQuotesCount == 0)
                return input.AsSpan(startIndex, endIndex - startIndex).ToString();

            return string.Create(
                endIndex - startIndex - escapedQuotesCount,
                (input, startIndex, endIndex),
                static (destination, state) =>
                {
                    var destinationIndex = 0;

                    for (var i = state.startIndex; i < state.endIndex; i++)
                    {
                        var c = state.input[i];
                        if (c == Quote && i + 1 < state.endIndex && state.input[i + 1] == Quote)
                            i++;

                        destination[destinationIndex++] = c;
                    }
                });
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

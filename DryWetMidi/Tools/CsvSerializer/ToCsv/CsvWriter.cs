using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvWriter : IDisposable
    {
        #region Fields

        private readonly StreamWriter? _streamWriter;
        private readonly List<string>? _records;
        private readonly Encoding _asyncEncoding = new UTF8Encoding(false, true);
        private readonly string _delimiterString;
        private readonly Func<byte, string> _byteFormatter;
        private readonly string _bytesArrayDelimiter;

        private bool _disposed = false;

        #endregion

        #region Constructor

        public CsvWriter(Stream stream, CsvSerializationSettings settings)
        {
            _streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), settings.BufferSize, true);
            _delimiterString = settings.Delimiter.ToString();
            _byteFormatter = GetByteFormatter(settings.BytesArrayFormat);
            _bytesArrayDelimiter = settings.BytesArrayDelimiter.ToString();
        }

        internal CsvWriter(CsvSerializationSettings settings)
        {
            _records = new List<string>();
            _delimiterString = settings.Delimiter.ToString();
            _byteFormatter = GetByteFormatter(settings.BytesArrayFormat);
            _bytesArrayDelimiter = settings.BytesArrayDelimiter.ToString();
        }

        #endregion

        #region Methods

        public void WriteRecord(IEnumerable<object> values)
        {
            var record = string.Join(_delimiterString, values.Select(ProcessValue));
            if (_streamWriter != null)
                _streamWriter.WriteLine(record);
            else
                _records!.Add(record);
        }

        internal async Task DrainAsync(Stream stream, CancellationToken cancellationToken)
        {
            foreach (var record in _records!)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bytes = _asyncEncoding.GetBytes(record + Environment.NewLine);
                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
            }
            _records.Clear();
        }

        public void WriteRecord(params object[] values)
        {
            WriteRecord((IEnumerable<object>)values);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private object ProcessValue(object value)
        {
            if (value == null)
                return string.Empty;

            var bytes = value as byte[];
            if (bytes != null)
                value = string.Join(_bytesArrayDelimiter, bytes.Select(b => _byteFormatter(b)));

            var s = value as string;
            if (s != null)
                value = CsvFormattingUtilities.EscapeString(s);

            return value;
        }

        private static Func<byte, string> GetByteFormatter(CsvBytesArrayFormat format)
        {
            if (format == CsvBytesArrayFormat.Decimal)
                return GetAsDecimal;

            if (format == CsvBytesArrayFormat.Hexadecimal)
                return GetAsHexadecimal;

            throw new NotImplementedException();
        }

        private static string GetAsDecimal(byte b)
        {
            return b.ToString();
        }

        private static string GetAsHexadecimal(byte b)
        {
            return Convert.ToString((int)b, 16).PadLeft(2, '0').ToUpperInvariant();
        }

        #endregion

        #region IDisposable

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _streamWriter?.Dispose();

            _disposed = true;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvWriter : IDisposable
    {
        #region Fields

        private readonly StreamWriter _streamWriter;
        private readonly string _delimiterString;
        private readonly Func<byte, string> _byteFormatter;

        private bool _disposed = false;

        #endregion

        #region Constructor

        public CsvWriter(Stream stream, CsvSerializationSettings settings)
        {
            _streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), settings.ReadWriteBufferSize, true);
            _delimiterString = settings.Delimiter.ToString();
            _byteFormatter = GetByteFormatter(settings.BytesArrayFormat);
        }

        #endregion

        #region Methods

        public void WriteRecord(IEnumerable<object> values)
        {
            _streamWriter.WriteLine(string.Join(_delimiterString, values.Select(ProcessValue)));
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

            // TODO: bytes delimiter
            var bytes = value as byte[];
            if (bytes != null)
                value = string.Join(" ", bytes.Select(b => _byteFormatter(b)));

            var s = value as string;
            if (s != null)
                return CsvFormattingUtilities.EscapeString(s);

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
                _streamWriter.Dispose();

            _disposed = true;
        }

        #endregion
    }
}

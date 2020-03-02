using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvWriter : IDisposable
    {
        #region Fields

        private readonly StreamWriter _streamWriter;
        private readonly char _delimiter;

        #endregion

        #region Constructor

        public CsvWriter(Stream stream, CsvSettings settings)
        {
            _streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true);
            _delimiter = settings.CsvDelimiter;
        }

        #endregion

        #region Methods

        public void WriteRecord(IEnumerable<object> values)
        {
            _streamWriter.WriteLine(string.Join(_delimiter.ToString(), values));
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _streamWriter.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}

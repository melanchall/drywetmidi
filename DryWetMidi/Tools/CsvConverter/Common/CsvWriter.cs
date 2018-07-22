using System;
using System.Collections.Generic;
using System.IO;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvWriter : IDisposable
    {
        #region Fields

        private readonly StreamWriter _streamWriter;
        private readonly char _delimiter;

        #endregion

        #region Constructor

        public CsvWriter(Stream stream, char delimiter)
        {
            _streamWriter = new StreamWriter(stream);
            _delimiter = delimiter;
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

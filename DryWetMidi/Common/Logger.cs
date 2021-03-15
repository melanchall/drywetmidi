using System;
using System.IO;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class Logger : IDisposable
    {
        #region Fields

        private static Logger _instance = null;

        private FileStream _fileStream;
        private StreamWriter _streamWriter;
        private bool _disposed;

        #endregion

        #region Constructor

        private Logger()
        {
        }

        #endregion

        #region Properties

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Logger();

                return _instance;
            }
        }

        #endregion

        #region Methods

        public void WriteLine(string filePath, string line)
        {
            if (_streamWriter == null)
            {
                _fileStream = File.OpenWrite(filePath);
                _streamWriter = new StreamWriter(_fileStream);
            }

            _streamWriter.WriteLine(line);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _streamWriter.Dispose();
                    _fileStream.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}

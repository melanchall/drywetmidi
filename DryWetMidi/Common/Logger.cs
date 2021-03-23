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
            EnsureStreamCreated(filePath);
            _streamWriter.WriteLine(line);
        }

        public void Write(string filePath, string text)
        {
            EnsureStreamCreated(filePath);
            _streamWriter.Write(text);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void EnsureStreamCreated(string filePath)
        {
            if (_streamWriter == null)
            {
                _fileStream = File.OpenWrite(filePath);
                _streamWriter = new StreamWriter(_fileStream);
            }
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

                _fileStream = null;
                _streamWriter = null;
                _disposed = true;
            }
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Melanchall.CheckDwmApi
{
    internal sealed class ReportWriter : IDisposable
    {
        private readonly FileStream _reportFileStream;
        private readonly StreamWriter _streamWriter;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private bool _disposed;

        public ReportWriter(string reportFilePath)
        {
            if (File.Exists(reportFilePath))
                File.Delete(reportFilePath);

            _reportFileStream = new FileStream(reportFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            _streamWriter = new StreamWriter(_reportFileStream, Encoding.UTF8);
        }

        public void WriteLine(string line)
        {
            Console.WriteLine(line);

            if (!_stopwatch.IsRunning)
                _stopwatch.Start();

            _streamWriter.WriteLine($"[{_stopwatch.Elapsed:mm\\:ss\\.fff}] {line}");
            _streamWriter.Flush();
        }

        public void Close()
        {
            _streamWriter.Flush();
            _reportFileStream.Flush();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Close();
                _reportFileStream.Dispose();
            }

            _disposed = true;
        }

        public void Dispose() =>
            Dispose(true);
    }
}

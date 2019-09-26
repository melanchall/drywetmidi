using System;
using System.Timers;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class RegularPrecisionTickGenerator : ITickGenerator
    {
        #region Events

        public event EventHandler TickGenerated;

        #endregion

        #region Fields

        private bool _disposed = false;
        private bool _started = false;

        private readonly Timer _timer;

        #endregion

        #region Constructor

        public RegularPrecisionTickGenerator(TimeSpan interval)
        {
            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Elapsed += OnElapsed;
        }

        #endregion

        #region Methods

        public void TryStart()
        {
            if (_started)
                return;

            _timer.Start();
            _started = true;
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            TickGenerated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _timer.Elapsed -= OnElapsed;
                _timer.Stop();
                _timer.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}

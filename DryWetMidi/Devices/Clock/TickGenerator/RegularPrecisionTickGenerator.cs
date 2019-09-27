using System;
using System.Timers;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class RegularPrecisionTickGenerator : ITickGenerator
    {
        #region Constants

        public static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);
        public static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(int.MaxValue);

        #endregion

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
            ThrowIfArgument.IsOutOfRange(nameof(interval),
                                         interval,
                                         MinInterval,
                                         MaxInterval,
                                         $"Interval is out of [{MinInterval}, {MaxInterval}] range.");

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
            if (!_started || _disposed)
                return;

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
                _timer.Stop();
                _timer.Elapsed -= OnElapsed;
                _timer.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}

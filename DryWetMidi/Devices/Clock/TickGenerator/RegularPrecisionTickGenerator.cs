using System;
using System.Timers;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Tick generator which uses <see cref="Timer"/> for ticking.
    /// </summary>
    public sealed class RegularPrecisionTickGenerator : ITickGenerator
    {
        #region Constants

        /// <summary>
        /// The smallest possible interval.
        /// </summary>
        public static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// The largest possible interval.
        /// </summary>
        public static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(int.MaxValue);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when new tick generated.
        /// </summary>
        public event EventHandler TickGenerated;

        #endregion

        #region Fields

        private bool _disposed = false;
        private bool _started = false;

        private readonly Timer _timer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegularPrecisionTickGenerator"/> with the specified
        /// interval.
        /// </summary>
        /// <param name="interval">Interval of ticking.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is out of valid range.</exception>
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

        /// <summary>
        /// Starts the tick generator if it's not started; otherwise does nothing.
        /// </summary>
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

        /// <summary>
        /// Releases all resources used by the current <see cref="RegularPrecisionTickGenerator"/>.
        /// </summary>
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

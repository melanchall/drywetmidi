using System;
using System.Timers;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Tick generator which uses <see cref="Timer"/> for ticking.
    /// </summary>
    public sealed class RegularPrecisionTickGenerator : TickGenerator
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

        #region Fields

        private Timer _timer;
        private bool _disposed = false;

        #endregion

        #region Overrides

        /// <summary>
        /// Starts a tick generator.
        /// </summary>
        /// <param name="interval">Interval between ticks.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is out of
        /// [<see cref="MinInterval"/>; <see cref="MaxInterval"/>] range.</exception>
        protected override void Start(TimeSpan interval)
        {
            ThrowIfArgument.IsOutOfRange(
                nameof(interval),
                interval,
                MinInterval,
                MaxInterval,
                $"Interval is out of [{MinInterval}, {MaxInterval}] range.");

            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Elapsed += OnElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Stops a tick generator.
        /// </summary>
        protected override void Stop()
        {
            _timer.Stop();
        }

        #endregion

        #region Methods

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsRunning || _disposed)
                return;

            GenerateTick();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current tick generator.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && IsRunning)
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

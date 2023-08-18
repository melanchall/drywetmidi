using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// MIDI clock used to drive playback or any timer-based object.
    /// </summary>
    public sealed class MidiClock : IDisposable
    {
        #region Constants

        private const double DefaultSpeed = 1.0;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when new tick generated.
        /// </summary>
        public event EventHandler Ticked;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly bool _startImmediately;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan _startTime = TimeSpan.Zero;
        private TimeSpan _elapsed = TimeSpan.Zero;

        private double _speed = DefaultSpeed;

        private readonly TickGenerator _tickGenerator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiClock"/> with the specified
        /// value indicating whether first tick should be generated immediately after clock started, and
        /// tick generator.
        /// </summary>
        /// <param name="startImmediately">A value indicating whether first tick should be generated
        /// immediately after clock started.</param>
        /// <param name="tickGenerator">Tick generator used as timer firing at the specified interval. Null for
        /// no tick generator.</param>
        /// <param name="interval">Interval of clock's ticking.</param>
        public MidiClock(bool startImmediately, TickGenerator tickGenerator, TimeSpan interval)
        {
            ThrowIfArgument.IsLessThan(nameof(interval), interval, TimeSpan.FromMilliseconds(1), "Interval is less than 1 ms.");

            _startImmediately = startImmediately;

            _tickGenerator = tickGenerator;
            if (_tickGenerator != null)
                _tickGenerator.TickGenerated += OnTickGenerated;

            Interval = interval;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="MidiClock"/>.
        /// </summary>
        ~MidiClock()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the interval of the current clock's ticking.
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        /// Gets a value indicating whether MIDI clock is currently running or not.
        /// </summary>
        public bool IsRunning => _stopwatch.IsRunning;

        /// <summary>
        /// Gets the current time of clock as <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the speed of clock, i.e. the speed of current time changing.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public double Speed
        {
            get { return _speed; }
            set
            {
                EnsureIsNotDisposed();
                ThrowIfArgument.IsNegative(nameof(value), value, "Speed is negative.");

                var start = IsRunning;

                Stop();

                _startTime = GetCurrentTime();
                _elapsed = _stopwatch.Elapsed;
                _speed = value;

                if (start)
                    Start();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts/resumes the clock.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public void Start()
        {
            EnsureIsNotDisposed();

            if (IsRunning)
                return;

            _tickGenerator?.TryStart(Interval);
            _stopwatch.Start();

            if (_startImmediately)
                OnTicked();
        }

        /// <summary>
        /// Stops the clock.Current time will not be changed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public void Stop()
        {
            EnsureIsNotDisposed();

            StopInternally();
        }

        /// <summary>
        /// Stops, sets current time to zero and starts the clock.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public void Restart()
        {
            EnsureIsNotDisposed();

            Stop();
            ResetCurrentTime();
            Start();
        }

        /// <summary>
        /// Resets the current time of the clock setting it to zero.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public void ResetCurrentTime()
        {
            EnsureIsNotDisposed();

            SetCurrentTime(TimeSpan.Zero);
        }

        /// <summary>
        /// Sets the current time of the clock.
        /// </summary>
        /// <param name="time">New current time of the clock.</param>
        /// <exception cref="ObjectDisposedException">The current <see cref="MidiClock"/> is disposed.</exception>
        public void SetCurrentTime(TimeSpan time)
        {
            EnsureIsNotDisposed();

            _stopwatch.Reset();
            _elapsed = TimeSpan.Zero;
            _startTime = time;
            CurrentTime = time;
        }

        /// <summary>
        /// Generates new clock's tick manually without pulse from tick generator.
        /// </summary>
        public void Tick()
        {
            if (!IsRunning || _disposed)
                return;

            CurrentTime = GetCurrentTime();
            OnTicked();
        }

        internal void StopInternally()
        {
            if (_disposed)
                return;

            _stopwatch.Stop();
            _tickGenerator?.TryStop();
        }

        internal void StopShortly()
        {
            _stopwatch.Stop();
        }

        private void OnTickGenerated(object sender, EventArgs e)
        {
            Tick();
        }

        private void OnTicked()
        {
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("MIDI clock is disposed.");
        }

        private TimeSpan GetCurrentTime()
        {
            var ticks = (_stopwatch.Elapsed - _elapsed).Ticks;
            if (ticks < 0)
                ticks = 0;

            return _startTime + new TimeSpan(MathUtilities.RoundToLong(ticks * Speed));
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="MidiClock"/>.
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
                if (_tickGenerator != null)
                {
                    _tickGenerator.TickGenerated -= OnTickGenerated;
                    _tickGenerator.Dispose();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}

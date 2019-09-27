using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiClock : IDisposable
    {
        #region Constants

        private const double DefaultSpeed = 1.0;

        #endregion

        #region Events

        public event EventHandler<TickedEventArgs> Ticked;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly bool _startImmediately;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan _startTime = TimeSpan.Zero;

        private double _speed = DefaultSpeed;

        private bool _started;

        private readonly ITickGenerator _tickGenerator;

        #endregion

        #region Constructor

        public MidiClock(bool startImmediately, ITickGenerator tickGenerator)
        {
            _startImmediately = startImmediately;

            _tickGenerator = tickGenerator;
            if (_tickGenerator != null)
                _tickGenerator.TickGenerated += OnTickGenerated;
        }

        #endregion

        #region Finalizer

        ~MidiClock()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsRunning => _stopwatch.IsRunning;

        public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

        public double Speed
        {
            get { return _speed; }
            set
            {
                EnsureIsNotDisposed();
                ThrowIfArgument.IsNegative(nameof(value), value, "Speed is negative.");

                var start = IsRunning;

                Stop();

                _startTime = _stopwatch.Elapsed;
                _speed = value;

                if (start)
                    Start();
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            EnsureIsNotDisposed();

            if (IsRunning)
                return;

            if (!_started)
                _tickGenerator?.TryStart();

            _stopwatch.Start();

            if (_startImmediately)
                OnTicked();

            _started = true;
        }

        public void Stop()
        {
            EnsureIsNotDisposed();

            _stopwatch.Stop();
        }

        public void Restart()
        {
            EnsureIsNotDisposed();

            Stop();
            Reset();
            Start();
        }

        public void Reset()
        {
            EnsureIsNotDisposed();

            SetCurrentTime(TimeSpan.Zero);
        }

        public void SetCurrentTime(TimeSpan time)
        {
            EnsureIsNotDisposed();

            _stopwatch.Reset();
            _startTime = time;
            CurrentTime = time;
        }

        public void Tick()
        {
            if (!IsRunning || _disposed)
                return;

            CurrentTime = _startTime + new TimeSpan(MathUtilities.RoundToLong(_stopwatch.Elapsed.Ticks * Speed));
            OnTicked();
        }

        private void OnTickGenerated(object sender, EventArgs e)
        {
            Tick();
        }

        private void OnTicked()
        {
            Ticked?.Invoke(this, new TickedEventArgs(CurrentTime));
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("MIDI clock is disposed.");
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

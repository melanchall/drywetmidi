using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class MidiClock : IDisposable
    {
        #region Events

        public event EventHandler<TickEventArgs> Tick;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly uint _interval;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private uint _resolution;
        private MidiTimerWinApi.TimeProc _tickCallback;
        private uint _timerId;

        private double _speed = 1.0;

        #endregion

        #region Constructor

        public MidiClock(uint interval)
        {
            _interval = interval;
        }

        #endregion

        #region Finalizer

        ~MidiClock()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsRunning { get; private set; }

        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;

        public TimeSpan CurrentTime { get; private set; } = TimeSpan.Zero;

        public double Speed
        {
            get { return _speed; }
            set
            {
                var start = IsRunning;

                IsRunning = false;
                _stopwatch.Stop();

                StartTime = _stopwatch.Elapsed;
                _speed = value;

                if (start)
                {
                    IsRunning = true;
                    _stopwatch.Start();
                }
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (IsRunning)
                return;

            // TODO: process errors

            var timeCaps = default(MidiTimerWinApi.TIMECAPS);
            MidiTimerWinApi.timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps));

            _resolution = Math.Min(Math.Max(timeCaps.wPeriodMin, _interval), timeCaps.wPeriodMax);
            _tickCallback = OnTick;

            _stopwatch.Start();

            MidiTimerWinApi.timeBeginPeriod(_resolution);
            _timerId = MidiTimerWinApi.timeSetEvent(_interval, _resolution, _tickCallback, IntPtr.Zero, MidiTimerWinApi.TIME_PERIODIC);

            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;

            // TODO: process errors

            _stopwatch.Stop();
            MidiTimerWinApi.timeEndPeriod(_resolution);
            MidiTimerWinApi.timeKillEvent(_timerId);
        }

        public void Restart()
        {
            Stop();
            Reset();
            Start();
        }

        public void Reset()
        {
            _stopwatch.Reset();
            StartTime = TimeSpan.Zero;
            CurrentTime = TimeSpan.Zero;
        }

        private void OnTick(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            if (!IsRunning)
                return;

            CurrentTime = StartTime + new TimeSpan(MathUtilities.RoundToLong((_stopwatch.Elapsed - StartTime).Ticks * Speed));
            Tick?.Invoke(this, new TickEventArgs(CurrentTime));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: prevent exceptions
            Stop();

            _disposed = true;
        }

        #endregion
    }
}

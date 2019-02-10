using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class MidiClock : IDisposable
    {
        #region Constants

        private const double DefaultSpeed = 1.0;
        private const uint NoTimerId = 0;

        #endregion

        #region Events

        public event EventHandler<TickEventArgs> Tick;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly uint _interval;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private uint _resolution;
        private MidiTimerWinApi.TimeProc _tickCallback;
        private uint _timerId = NoTimerId;

        private double _speed = DefaultSpeed;

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

        public bool IsRunning => _stopwatch.IsRunning;

        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;

        public TimeSpan CurrentTime { get; set; } = TimeSpan.Zero;

        public double Speed
        {
            get { return _speed; }
            set
            {
                var start = IsRunning;

                Stop();

                StartTime = _stopwatch.Elapsed;
                _speed = value;

                if (start)
                    Start();
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            if (IsRunning)
                return;

            if (_timerId == NoTimerId)
            {

                var timeCaps = default(MidiTimerWinApi.TIMECAPS);
                ProcessMmResult(MidiTimerWinApi.timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps)));

                _resolution = Math.Min(Math.Max(timeCaps.wPeriodMin, _interval), timeCaps.wPeriodMax);
                _tickCallback = OnTick;

                ProcessMmResult(MidiTimerWinApi.timeBeginPeriod(_resolution));
                _timerId = MidiTimerWinApi.timeSetEvent(_interval, _resolution, _tickCallback, IntPtr.Zero, MidiTimerWinApi.TIME_PERIODIC);
                if (_timerId == 0)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    throw new MidiDeviceException("Unable to initialize MIDI clock.", new Win32Exception(errorCode));
                }
            }

            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
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

            CurrentTime = StartTime + new TimeSpan(MathUtilities.RoundToLong(_stopwatch.Elapsed.Ticks * Speed));
            Tick?.Invoke(this, new TickEventArgs(CurrentTime));
        }

        private static void ProcessMmResult(uint mmResult)
        {
            switch (mmResult)
            {
                case MidiWinApi.MMSYSERR_ERROR:
                case MidiWinApi.TIMERR_NOCANDO:
                    throw new MidiDeviceException("Error occurred on MIDI clock.");
            }
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
            }

            if (_timerId != NoTimerId)
            {
                MidiTimerWinApi.timeEndPeriod(_resolution);
                MidiTimerWinApi.timeKillEvent(_timerId);
            }

            _disposed = true;
        }

        #endregion
    }
}

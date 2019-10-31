using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Tick generator providing most accurate ticking, allowing firing intervals of 1 ms which
    /// is the smallest possible one.
    /// </summary>
    public sealed class HighPrecisionTickGenerator : ITickGenerator
    {
        #region Constants

        /// <summary>
        /// The smallest possible interval.
        /// </summary>
        public static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// The largest possible interval.
        /// </summary>
        public static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(uint.MaxValue);

        private const uint NoTimerId = 0;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when new tick generated.
        /// </summary>
        public event EventHandler TickGenerated;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly uint _interval;
        private uint _resolution;
        private MidiTimerWinApi.TimeProc _tickCallback;
        private uint _timerId = NoTimerId;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HighPrecisionTickGenerator"/> with the specified
        /// interval.
        /// </summary>
        /// <param name="interval">Interval of ticking.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is out of valid range.</exception>
        public HighPrecisionTickGenerator(TimeSpan interval)
        {
            ThrowIfArgument.IsOutOfRange(nameof(interval),
                                         interval,
                                         MinInterval,
                                         MaxInterval,
                                         $"Interval is out of [{MinInterval}, {MaxInterval}] range.");

            _interval = (uint)interval.TotalMilliseconds;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="HighPrecisionTickGenerator"/>.
        /// </summary>
        ~HighPrecisionTickGenerator()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the tick generator if it's not started; otherwise does nothing.
        /// </summary>
        /// <exception cref="MidiDeviceException">An error occurred on tick generator.</exception>
        public void TryStart()
        {
            if (_timerId != NoTimerId)
                return;

            var timeCaps = default(MidiTimerWinApi.TIMECAPS);
            ProcessMmResult(MidiTimerWinApi.timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps)));

            _resolution = Math.Min(Math.Max(timeCaps.wPeriodMin, _interval), timeCaps.wPeriodMax);
            _tickCallback = OnTick;

            ProcessMmResult(MidiTimerWinApi.timeBeginPeriod(_resolution));
            _timerId = MidiTimerWinApi.timeSetEvent(_interval, _resolution, _tickCallback, IntPtr.Zero, MidiTimerWinApi.TIME_PERIODIC);
            if (_timerId == 0)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new MidiDeviceException("Unable to start tick generator.", new Win32Exception(errorCode));
            }
        }

        private void OnTick(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            if (_timerId == NoTimerId || _disposed)
                return;

            TickGenerated?.Invoke(this, EventArgs.Empty);
        }

        private static void ProcessMmResult(uint mmResult)
        {
            switch (mmResult)
            {
                case MidiWinApi.MMSYSERR_ERROR:
                case MidiWinApi.TIMERR_NOCANDO:
                    throw new MidiDeviceException("Error occurred on high precision MIDI tick generator.");
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="HighPrecisionTickGenerator"/>.
        /// </summary>
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

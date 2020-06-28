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
    public sealed class HighPrecisionTickGenerator : TickGenerator
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

        #region Fields

        private bool _disposed = false;

        private uint _resolution;
        private MidiTimerWinApi.TimeProc _tickCallback;
        private uint _timerId = NoTimerId;

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

        #region Overrides

        /// <summary>
        /// Starts a tick generator.
        /// </summary>
        /// <param name="interval">Interval between ticks.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is out of
        /// [<see cref="MinInterval"/>; <see cref="MaxInterval"/>] range.</exception>
        protected override void Start(TimeSpan interval)
        {
            ThrowIfArgument.IsOutOfRange(nameof(interval),
                                         interval,
                                         MinInterval,
                                         MaxInterval,
                                         $"Interval is out of [{MinInterval}, {MaxInterval}] range.");

            var intervalInMilliseconds = (uint)interval.TotalMilliseconds;

            var timeCaps = default(MidiTimerWinApi.TIMECAPS);
            ProcessMmResult(MidiTimerWinApi.timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps)));

            _resolution = Math.Min(Math.Max(timeCaps.wPeriodMin, intervalInMilliseconds), timeCaps.wPeriodMax);
            _tickCallback = OnTick;

            ProcessMmResult(MidiTimerWinApi.timeBeginPeriod(_resolution));
            _timerId = MidiTimerWinApi.timeSetEvent(intervalInMilliseconds, _resolution, _tickCallback, IntPtr.Zero, MidiTimerWinApi.TIME_PERIODIC);
            if (_timerId == NoTimerId)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new MidiDeviceException("Unable to start tick generator.", new Win32Exception(errorCode));
            }
        }

        #endregion

        #region Methods

        private void OnTick(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            if (!IsRunning || _disposed)
                return;

            GenerateTick();
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
        /// Releases all resources used by the current tick generator.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current tick generator.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            if (IsRunning)
            {
                MidiTimerWinApi.timeEndPeriod(_resolution);
                MidiTimerWinApi.timeKillEvent(_timerId);
            }

            _disposed = true;
        }

        #endregion
    }
}

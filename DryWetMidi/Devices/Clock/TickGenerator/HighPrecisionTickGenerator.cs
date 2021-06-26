using System;
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

        private TickGeneratorApi.TimerCallback_Winmm _tickCallbackWinmm;
        private TickGeneratorApi.TimerCallback_Apple _tickCallbackApple;
        private IntPtr _info;

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

            var intervalInMilliseconds = (int)interval.TotalMilliseconds;

            var apiType = TickGeneratorApiProvider.Api.Api_GetApiType();
            var result = default(TickGeneratorApi.TG_STARTRESULT);

            switch (apiType)
            {
                case TickGeneratorApi.API_TYPE.API_TYPE_WINMM:
                    {
                        _tickCallbackWinmm = OnTick_Winmm;
                        result = TickGeneratorApiProvider.Api.Api_StartHighPrecisionTickGenerator_Winmm(
                            intervalInMilliseconds,
                            _tickCallbackWinmm,
                            out _info);
                    }
                    break;
                case TickGeneratorApi.API_TYPE.API_TYPE_APPLE:
                    {
                        _tickCallbackApple = OnTick_Apple;
                        result = TickGeneratorApiProvider.Api.Api_StartHighPrecisionTickGenerator_Apple(
                            intervalInMilliseconds,
                            _tickCallbackApple,
                            out _info);
                    }
                    break;
            }

            if (result != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                throw new InvalidOperationException($"FAILED START: {result}");
        }

        /// <summary>
        /// Stops a tick generator.
        /// </summary>
        protected override void Stop()
        {
            if (_info == IntPtr.Zero)
                return;

            var result = TickGeneratorApiProvider.Api.Api_StopHighPrecisionTickGenerator(_info);
            if (result != TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK)
                throw new InvalidOperationException($"FAILED STOP: {result}");
        }

        #endregion

        #region Methods

        private void OnTick_Winmm(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            OnTick();
        }

        private void OnTick_Apple(IntPtr timer, IntPtr info)
        {
            OnTick();
        }

        private void OnTick()
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
                Stop();

            _disposed = true;
        }

        #endregion
    }
}

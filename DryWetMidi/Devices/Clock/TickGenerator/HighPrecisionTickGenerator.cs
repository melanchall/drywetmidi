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
        public static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(int.MaxValue);

        #endregion

        #region Fields

        private bool _disposed = false;

        private TickGeneratorApi.TimerCallback_Win _tickCallback_Win;
        private TickGeneratorApi.TimerCallback_Mac _tickCallback_Mac;
        private IntPtr _tickGeneratorInfo;

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

            var apiType = CommonApiProvider.Api.Api_GetApiType();
            var result = default(TickGeneratorApi.TG_STARTRESULT);

            switch (apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    result = StartHighPrecisionTickGenerator_Win(intervalInMilliseconds, out _tickGeneratorInfo);
                    break;
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    result = StartHighPrecisionTickGenerator_Mac(intervalInMilliseconds, out _tickGeneratorInfo);
                    break;
            }

            if (result != TickGeneratorApi.TG_STARTRESULT.TG_STARTRESULT_OK)
                throw new TickGeneratorException("Failed to start high-precision tick generator.", (int)result);
        }

        /// <summary>
        /// Stops a tick generator.
        /// </summary>
        protected override void Stop()
        {
            var result = StopInternal();
            if (result != TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK)
                throw new TickGeneratorException("Failed to stop high-precision tick generator.", (int)result);
        }

        #endregion

        #region Methods

        private void OnTick_Win(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            OnTick();
        }

        private void OnTick_Mac()
        {
            OnTick();
        }

        private void OnTick()
        {
            if (!IsRunning || _disposed)
                return;

            GenerateTick();
        }

        private TickGeneratorApi.TG_STOPRESULT StopInternal()
        {
            if (_tickGeneratorInfo == IntPtr.Zero)
                return TickGeneratorApi.TG_STOPRESULT.TG_STOPRESULT_OK;

            var result = TickGeneratorApiProvider.Api.Api_StopHighPrecisionTickGenerator(_tickGeneratorInfo);
            _tickGeneratorInfo = IntPtr.Zero;
            return result;
        }

        private TickGeneratorApi.TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int intervalInMilliseconds, out IntPtr tickGeneratorInfo)
        {
            _tickCallback_Win = OnTick_Win;
            return TickGeneratorApiProvider.Api.Api_StartHighPrecisionTickGenerator_Win(
                intervalInMilliseconds,
                _tickCallback_Win,
                out tickGeneratorInfo);
        }

        private TickGeneratorApi.TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int intervalInMilliseconds, out IntPtr tickGeneratorInfo)
        {
            _tickCallback_Mac = OnTick_Mac;
            return TickGeneratorApiProvider.Api.Api_StartHighPrecisionTickGenerator_Mac(
                intervalInMilliseconds,
                _tickCallback_Mac,
                out tickGeneratorInfo);
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

            StopInternal();

            _disposed = true;
        }

        #endregion
    }
}

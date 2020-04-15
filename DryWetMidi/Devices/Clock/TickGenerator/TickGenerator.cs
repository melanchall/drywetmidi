using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Tick generator which provides ticking with the specified interval.
    /// </summary>
    public abstract class TickGenerator : IDisposable
    {
        #region Events

        internal event EventHandler TickGenerated;

        #endregion

        #region Properties

        protected bool IsRunning { get; set; }

        #endregion

        #region Methods

        internal void TryStart(TimeSpan interval)
        {
            if (IsRunning)
                return;

            Start(interval);
            IsRunning = true;
        }

        protected void GenerateTick()
        {
            TickGenerated?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void Start(TimeSpan interval);

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}

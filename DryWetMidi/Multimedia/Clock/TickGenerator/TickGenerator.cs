using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Tick generator which provides ticking with the specified interval.
    /// </summary>
    public abstract class TickGenerator : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs on tick generator's tick.
        /// </summary>
        public event EventHandler TickGenerated;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current tick generator is currently running or not.
        /// </summary>
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

        internal void TryStop()
        {
            if (!IsRunning)
                return;

            Stop();
            IsRunning = false;
        }

        /// <summary>
        /// Generates a tick firing the <see cref="TickGenerated"/> event.
        /// </summary>
        protected void GenerateTick()
        {
            TickGenerated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Starts a tick generator.
        /// </summary>
        /// <param name="interval">Interval between ticks.</param>
        protected abstract void Start(TimeSpan interval);

        /// <summary>
        /// Stops a tick generator.
        /// </summary>
        protected abstract void Stop();

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current tick generator.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases all resources used by the current tick generator.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}

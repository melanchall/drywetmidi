using System;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Holds current time for the <see cref="MidiClock.Ticked"/> event.
    /// </summary>
    public sealed class TickedEventArgs : EventArgs
    {
        #region Constructor

        internal TickedEventArgs(TimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current time of a MIDI clock.
        /// </summary>
        public TimeSpan Time { get; }

        #endregion
    }
}

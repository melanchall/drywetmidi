using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a change of a parameter's value at some time.
    /// </summary>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public sealed class ValueChange<TValue> : ITimedObject
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueChange{T}"/> with the specified
        /// time of change and new value.
        /// </summary>
        /// <param name="time">MIDI time when value is changed.</param>
        /// <param name="value">New value that will last until next value change.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        internal ValueChange(long time, TValue value)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Time = time;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the MIDI time when value is changed.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Gets the new value that will last until next value change.
        /// </summary>
        public TValue Value { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Value} at {Time}";
        }

        #endregion
    }
}

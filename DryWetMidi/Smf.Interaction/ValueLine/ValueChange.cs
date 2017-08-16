using Melanchall.DryWetMidi.Common;
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
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(value), value);

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

        #region Operators

        /// <summary>
        /// Determines if two <see cref="ValueChange{TValue}"/> objects are equal.
        /// </summary>
        /// <param name="change1">The first <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <param name="change2">The second <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <returns>true if the value changes are equal, false otherwise.</returns>
        public static bool operator ==(ValueChange<TValue> change1, ValueChange<TValue> change2)
        {
            if (ReferenceEquals(change1, change2))
                return true;

            if (ReferenceEquals(null, change1) || ReferenceEquals(null, change2))
                return false;

            return change1.Time == change2.Time &&
                   change1.Value.Equals(change2.Value);
        }

        /// <summary>
        /// Determines if two <see cref="ValueChange{TValue}"/> objects are not equal.
        /// </summary>
        /// <param name="change1">The first <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <param name="change2">The second <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <returns>false if the value changes are equal, true otherwise.</returns>
        public static bool operator !=(ValueChange<TValue> change1, ValueChange<TValue> change2)
        {
            return !(change1 == change2);
        }

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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as ValueChange<TValue>);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Time.GetHashCode() ^ Value.GetHashCode();
        }

        #endregion
    }
}

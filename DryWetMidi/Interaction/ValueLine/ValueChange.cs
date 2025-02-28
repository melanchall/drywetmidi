using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a change of a parameter's value at some time.
    /// </summary>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public sealed class ValueChange<TValue> : ITimedObject
    {
        #region Fields

        private readonly long _time;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueChange{T}"/> with the specified
        /// time of change and new value.
        /// </summary>
        /// <param name="time">MIDI time when value is changed.</param>
        /// <param name="value">New value that will last until next value change.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        internal ValueChange(long time, TValue value)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(value), value);

            _time = time;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the MIDI time when value is changed.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the time in different representations.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Setting time of value change object is not allowed.</exception>
        public long Time
        {
            get { return _time; }
            set { throw new InvalidOperationException("Setting time of value change object is not allowed."); }
        }

        /// <summary>
        /// Gets the new value that will last until next value change.
        /// </summary>
        public TValue Value { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public ITimedObject Clone()
        {
            return new ValueChange<TValue>(Time, Value);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="ValueChange{TValue}"/> objects are equal.
        /// </summary>
        /// <param name="change1">The first <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <param name="change2">The second <see cref="ValueChange{TValue}"/> to compare.</param>
        /// <returns><c>true</c> if the value changes are equal, <c>false</c> otherwise.</returns>
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
        /// <returns><c>false</c> if the value changes are equal, <c>true</c> otherwise.</returns>
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
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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
            unchecked
            {
                var result = 17;
                result = result * 23 + Time.GetHashCode();
                result = result * 23 + Value.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}

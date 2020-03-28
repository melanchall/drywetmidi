using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a result of summation or subtraction of two <see cref="ITimeSpan"/>.
    /// </summary>
    public sealed class MathTimeSpan : ITimeSpan
    {
        #region Constants

        private const string TimeModeString = "T";
        private const string LengthModeString = "L";

        private static readonly Dictionary<TimeSpanMode, Tuple<string, string>> ModeStrings =
            new Dictionary<TimeSpanMode, Tuple<string, string>>
            {
                [TimeSpanMode.TimeTime] = Tuple.Create(TimeModeString, TimeModeString),
                [TimeSpanMode.TimeLength] = Tuple.Create(TimeModeString, LengthModeString),
                [TimeSpanMode.LengthLength] = Tuple.Create(LengthModeString, LengthModeString),
            };

        #endregion

        #region Constructor

        internal MathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation, TimeSpanMode mode)
        {
            TimeSpan1 = timeSpan1;
            TimeSpan2 = timeSpan2;
            Operation = operation;
            Mode = mode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the first <see cref="ITimeSpan"/>.
        /// </summary>
        public ITimeSpan TimeSpan1 { get; }

        /// <summary>
        /// Gets the second <see cref="ITimeSpan"/>.
        /// </summary>
        public ITimeSpan TimeSpan2 { get; }

        /// <summary>
        /// Gets the mathematical operation between <see cref="TimeSpan1"/> and <see cref="TimeSpan2"/>.
        /// </summary>
        public MathOperation Operation { get; }

        /// <summary>
        /// Get the mode of the mathematical operation represented by the current <see cref="MathTimeSpan"/>.
        /// </summary>
        public TimeSpanMode Mode { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="MathTimeSpan"/> objects are equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MathTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MathTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if time spans are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(MathTimeSpan timeSpan1, MathTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.TimeSpan1.Equals(timeSpan2.TimeSpan1) &&
                   timeSpan1.TimeSpan2.Equals(timeSpan2.TimeSpan2) &&
                   timeSpan1.Operation == timeSpan2.Operation &&
                   timeSpan1.Mode == timeSpan2.Mode;
        }

        /// <summary>
        /// Determines if two <see cref="MathTimeSpan"/> objects are not equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MathTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MathTimeSpan"/> to compare.</param>
        /// <returns><c>false</c> if time spans are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(MathTimeSpan timeSpan1, MathTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var operationString = Operation == MathOperation.Add
                ? "+"
                : "-";

            var modeStrings = ModeStrings[Mode];

            return $"({TimeSpan1}{modeStrings.Item1} {operationString} {TimeSpan2}{modeStrings.Item2})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as MathTimeSpan);
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
                result = result * 23 + TimeSpan1.GetHashCode();
                result = result * 23 + TimeSpan2.GetHashCode();
                result = result * 23 + Operation.GetHashCode();
                result = result * 23 + Mode.GetHashCode();
                return result;
            }
        }

        #endregion

        #region ITimeSpan

        /// <summary>
        /// Adds a time span to the current one.
        /// </summary>
        /// <remarks>
        /// If <paramref name="timeSpan"/> and the current time span have the same type,
        /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
        /// </remarks>
        /// <param name="timeSpan">Time span to add to the current one.</param>
        /// <param name="mode">Mode of the operation that defines meaning of time spans the
        /// operation will be performed on.</param>
        /// <returns>Time span that is a sum of the <paramref name="timeSpan"/> and the
        /// current time span.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is invalid.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="mode"/> specified an invalid value.</exception>
        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            return TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        /// <summary>
        /// Subtracts a time span from the current one.
        /// </summary>
        /// <remarks>
        /// If <paramref name="timeSpan"/> and the current time span have the same type,
        /// the result time span will be of this type too; otherwise - of the <see cref="MathTimeSpan"/>.
        /// </remarks>
        /// <param name="timeSpan">Time span to subtract from the current one.</param>
        /// <param name="mode">Mode of the operation that defines meaning of time spans the
        /// operation will be performed on.</param>
        /// <returns>Time span that is a difference between the <paramref name="timeSpan"/> and the
        /// current time span.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is invalid.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="mode"/> specified an invalid value.</exception>
        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            return TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        /// <summary>
        /// Stretches the current time span by multiplying its length by the specified multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier to stretch the time span by.</param>
        /// <returns>Time span that is the current time span stretched by the <paramref name="multiplier"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is negative.</exception>
        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathTimeSpan(TimeSpan1.Multiply(multiplier),
                                    TimeSpan2.Multiply(multiplier),
                                    Operation,
                                    Mode);
        }

        /// <summary>
        /// Shrinks the current time span by dividing its length by the specified divisor.
        /// </summary>
        /// <param name="divisor">Divisor to shrink the time span by.</param>
        /// <returns>Time span that is the current time span shrinked by the <paramref name="divisor"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is zero or negative.</exception>
        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MathTimeSpan(TimeSpan1.Divide(divisor),
                                    TimeSpan2.Divide(divisor),
                                    Operation,
                                    Mode);
        }

        /// <summary>
        /// Clones the current time span.
        /// </summary>
        /// <returns>Copy of the current time span.</returns>
        public ITimeSpan Clone()
        {
            return new MathTimeSpan(TimeSpan1.Clone(), TimeSpan2.Clone(), Operation, Mode);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns><para>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>This instance precedes <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>This instance follows <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(object other)
        {
            throw new InvalidOperationException("Cannot compare MathTimeSpan.");
        }

        #endregion
    }
}

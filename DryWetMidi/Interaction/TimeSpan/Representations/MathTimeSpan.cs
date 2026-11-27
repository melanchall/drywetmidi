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

        private static readonly Dictionary<TimeSpanMode, (string FirstTimeSpanMode, string SecondTimeSpanMode)> ModeStrings = new()
        {
            [TimeSpanMode.TimeTime] = (TimeModeString, TimeModeString),
            [TimeSpanMode.TimeLength] = (TimeModeString, LengthModeString),
            [TimeSpanMode.LengthLength] = (LengthModeString, LengthModeString),
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
        /// Gets the mode of the mathematical operation represented by the current <see cref="MathTimeSpan"/>.
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
        public static bool operator ==(MathTimeSpan? timeSpan1, MathTimeSpan? timeSpan2)
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
        public static bool operator !=(MathTimeSpan? timeSpan1, MathTimeSpan? timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var operationString = Operation == MathOperation.Add
                ? "+"
                : "-";

            var modeStrings = ModeStrings[Mode];

            return $"({TimeSpan1}{modeStrings.Item1} {operationString} {TimeSpan2}{modeStrings.Item2})";
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this == (obj as MathTimeSpan);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            return TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        /// <inheritdoc/>
        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            return TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        /// <inheritdoc/>
        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathTimeSpan(TimeSpan1.Multiply(multiplier),
                                    TimeSpan2.Multiply(multiplier),
                                    Operation,
                                    Mode);
        }

        /// <inheritdoc/>
        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MathTimeSpan(TimeSpan1.Divide(divisor),
                                    TimeSpan2.Divide(divisor),
                                    Operation,
                                    Mode);
        }

        /// <inheritdoc/>
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
        public int CompareTo(object? other)
        {
            throw new InvalidOperationException("Cannot compare MathTimeSpan.");
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
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
        /// <returns>true if time spans are equal, false otherwise.</returns>
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
        /// <returns>false if time spans are equal, true otherwise.</returns>
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
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
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
            return TimeSpan1.GetHashCode() ^
                   TimeSpan2.GetHashCode() ^
                   Operation.GetHashCode() ^
                   Mode.GetHashCode();
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
        /// <exception cref="ArgumentException"><paramref name="mode"/> is invalid.</exception>
        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

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
        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        /// <summary>
        /// Stretches the current time span by multiplying its length by the specified multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier to stretch the time span by.</param>
        /// <returns>Time span that is the current time span stretched by the <paramref name="multiplier"/>.</returns>
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
        /// Compares the current instance with another object of the same type and returns an integer
        /// that indicates whether the current instance precedes, follows, or occurs in the same
        /// position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings: Value Meaning Less than zero This instance precedes obj
        /// in the sort order. Zero This instance occurs in the same position in the sort order as obj.
        /// Greater than zero This instance follows obj in the sort order.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not the same type as this instance.</exception>
        public int CompareTo(object obj)
        {
            throw new InvalidOperationException("Cannot compare MathTimeSpan.");
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MusicalTimeSpan : ITimeSpan
    {
        #region Constructor

        public MusicalTimeSpan(Fraction fraction)
        {
            ThrowIfArgument.IsNull(nameof(fraction), fraction);

            Fraction = fraction;
        }

        #endregion

        #region Properties

        public Fraction Fraction { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MusicalTimeSpan timeSpan)
        {
            return MusicalTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static MusicalTimeSpan Parse(string input)
        {
            var parsingResult = MusicalTimeSpanParser.TryParse(input, out var timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static implicit operator MusicalTimeSpan(Fraction fraction)
        {
            return new MusicalTimeSpan(fraction);
        }

        public static implicit operator Fraction(MusicalTimeSpan timeSpan)
        {
            return timeSpan.Fraction;
        }

        public static bool operator ==(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.Fraction == timeSpan2.Fraction;
        }

        public static bool operator !=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static MusicalTimeSpan operator +(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new MusicalTimeSpan(timeSpan1.Fraction + timeSpan2.Fraction);
        }

        public static MusicalTimeSpan operator -(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1.Fraction < timeSpan2.Fraction)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MusicalTimeSpan(timeSpan1.Fraction - timeSpan2.Fraction);
        }

        public static bool operator <(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.Fraction < timeSpan2.Fraction;
        }

        public static bool operator >(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.Fraction > timeSpan2.Fraction;
        }

        public static bool operator <=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.Fraction <= timeSpan2.Fraction;
        }

        public static bool operator >=(MusicalTimeSpan timeSpan1, MusicalTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.Fraction >= timeSpan2.Fraction;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as MusicalTimeSpan);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Fraction.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Fraction.ToString();
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var musicalTimeSpan = timeSpan as MusicalTimeSpan;
            return musicalTimeSpan != null
                ? this + musicalTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var musicalTimeSpan = timeSpan as MusicalTimeSpan;
            return musicalTimeSpan != null
                ? this - musicalTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MusicalTimeSpan(Fraction * multiplier);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MusicalTimeSpan(Fraction / divisor);
        }

        #endregion
    }
}

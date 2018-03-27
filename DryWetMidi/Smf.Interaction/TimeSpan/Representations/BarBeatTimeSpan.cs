using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents bar/beat time span which represents bars, beats and ticks.
    /// </summary>
    public sealed class BarBeatTimeSpan : ITimeSpan
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTimeSpan"/>.
        /// </summary>
        public BarBeatTimeSpan()
            : this(0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTimeSpan"/> with the specified
        /// number of bars and beats.
        /// </summary>
        /// <param name="bars">The number of bars.</param>
        /// <param name="beats">The number of beats.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative.</exception>
        public BarBeatTimeSpan(long bars, long beats)
            : this(bars, beats, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTimeSpan"/> with the specified
        /// number of bars, beats and ticks.
        /// </summary>
        /// <param name="bars">The number of bars.</param>
        /// <param name="beats">The number of beats.</param>
        /// <param name="ticks">The number of ticks.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative. -or-
        /// <paramref name="beats"/> is negative. -or- <paramref name="ticks"/> is negative.</exception>
        public BarBeatTimeSpan(long bars, long beats, long ticks)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNegative(nameof(beats), beats, "Beats number is negative.");
            ThrowIfArgument.IsNegative(nameof(ticks), ticks, "Ticks number is negative.");

            Bars = bars;
            Beats = beats;
            Ticks = ticks;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the bars component of the time represented by the current <see cref="BarBeatTimeSpan"/>.
        /// </summary>
        public long Bars { get; }

        /// <summary>
        /// Gets the beats component of the time represented by the current <see cref="BarBeatTimeSpan"/>.
        /// </summary>
        public long Beats { get; }

        /// <summary>
        /// Gets the ticks component of the time represented by the current <see cref="BarBeatTimeSpan"/>.
        /// </summary>
        public long Ticks { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a bar/beat time span to its <see cref="BarBeatTimeSpan"/>
        /// equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="BarBeatTimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out BarBeatTimeSpan timeSpan)
        {
            return BarBeatTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        /// <summary>
        /// Converts the string representation of a bar/beat time span to its <see cref="BarBeatTimeSpan"/>
        /// equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="BarBeatTimeSpan"/> equivalent to the time span contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static BarBeatTimeSpan Parse(string input)
        {
            BarBeatTimeSpan timeSpan;
            var parsingResult = BarBeatTimeSpanParser.TryParse(input, out timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        private static void CalculateDifferencies(BarBeatTimeSpan timeSpan1,
                                                  BarBeatTimeSpan timeSpan2,
                                                  out long barsDifference,
                                                  out long beatsDifference,
                                                  out long ticksDifference)
        {
            barsDifference = timeSpan1.Bars - timeSpan2.Bars;
            beatsDifference = timeSpan1.Beats - timeSpan2.Beats;
            ticksDifference = timeSpan1.Ticks - timeSpan2.Ticks;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="BarBeatTimeSpan"/> objects are equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>true if time spans are equal, false otherwise.</returns>
        public static bool operator ==(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.Bars == timeSpan2.Bars &&
                   timeSpan1.Beats == timeSpan2.Beats &&
                   timeSpan1.Ticks == timeSpan2.Ticks;
        }

        /// <summary>
        /// Determines if two <see cref="BarBeatTimeSpan"/> objects are not equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>false if time spans are equal, true otherwise.</returns>
        public static bool operator !=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        /// <summary>
        /// Adds two specified <see cref="BarBeatTimeSpan"/> instances.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to add.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to add.</param>
        /// <returns>An object whose value is the sum of the values of <paramref name="timeSpan1"/> and
        /// <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static BarBeatTimeSpan operator +(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new BarBeatTimeSpan(timeSpan1.Bars + timeSpan2.Bars,
                                       timeSpan1.Beats + timeSpan2.Beats,
                                       timeSpan1.Ticks + timeSpan2.Ticks);
        }

        /// <summary>
        /// Subtracts a specified <see cref="BarBeatTimeSpan"/> from another one.
        /// </summary>
        /// <param name="timeSpan1">The minuend.</param>
        /// <param name="timeSpan2">The subtrahend.</param>
        /// <returns>An object whose value is the result of the value of <paramref name="timeSpan1"/> minus
        /// the value of <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="timeSpan1"/> is less than <paramref name="timeSpan2"/>.</exception>
        public static BarBeatTimeSpan operator -(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new BarBeatTimeSpan(timeSpan1.Bars - timeSpan2.Bars,
                                       timeSpan1.Beats - timeSpan2.Beats,
                                       timeSpan1.Ticks - timeSpan2.Ticks);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTimeSpan"/> is less than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is less than the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator <(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            long barsDelta, beatsDelta, ticksDelta;
            CalculateDifferencies(timeSpan1, timeSpan2, out barsDelta, out beatsDelta, out ticksDelta);
            return barsDelta < 0 || (barsDelta == 0 && (beatsDelta < 0 || (beatsDelta == 0 && ticksDelta < 0)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTimeSpan"/> is greater than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator >(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            long barsDelta, beatsDelta, ticksDelta;
            CalculateDifferencies(timeSpan1, timeSpan2, out barsDelta, out beatsDelta, out ticksDelta);
            return barsDelta > 0 || (barsDelta == 0 && (beatsDelta > 0 || (beatsDelta == 0 && ticksDelta > 0)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTimeSpan"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is less than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator <=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            long barsDelta, beatsDelta, ticksDelta;
            CalculateDifferencies(timeSpan1, timeSpan2, out barsDelta, out beatsDelta, out ticksDelta);
            return barsDelta < 0 || (barsDelta == 0 && (beatsDelta < 0 || (beatsDelta == 0 && ticksDelta <= 0)));
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTimeSpan"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTimeSpan"/> to compare.</param>
        /// <returns>true if the value of <paramref name="timeSpan1"/> is greater than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan1"/> is null. -or-
        /// <paramref name="timeSpan2"/> is null.</exception>
        public static bool operator >=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            long barsDelta, beatsDelta, ticksDelta;
            CalculateDifferencies(timeSpan1, timeSpan2, out barsDelta, out beatsDelta, out ticksDelta);
            return barsDelta < 0 || (barsDelta == 0 && (beatsDelta < 0 || (beatsDelta == 0 && ticksDelta >= 0)));
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
            return this == (obj as BarBeatTimeSpan);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Bars.GetHashCode() ^ Beats.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Bars}.{Beats}.{Ticks}";
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

            var barBeatTimeSpan = timeSpan as BarBeatTimeSpan;
            return barBeatTimeSpan != null
                ? this + barBeatTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
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

            var barBeatTimeSpan = timeSpan as BarBeatTimeSpan;
            return barBeatTimeSpan != null
                ? this - barBeatTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        /// <summary>
        /// Stretches the current time span by multiplying its length by the specified multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplier to stretch the time span by.</param>
        /// <returns>Time span that is the current time span stretched by the <paramref name="multiplier"/>.</returns>
        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new BarBeatTimeSpan((long)Math.Round(Bars * multiplier, MidpointRounding.AwayFromZero),
                                       (long)Math.Round(Beats * multiplier, MidpointRounding.AwayFromZero),
                                       (long)Math.Round(Ticks * multiplier, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Shrinks the current time span by dividing its length by the specified divisor.
        /// </summary>
        /// <param name="divisor">Divisor to shrink the time span by.</param>
        /// <returns>Time span that is the current time span shrinked by the <paramref name="divisor"/>.</returns>
        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new BarBeatTimeSpan((long)Math.Round(Bars / divisor, MidpointRounding.AwayFromZero),
                                       (long)Math.Round(Beats / divisor, MidpointRounding.AwayFromZero),
                                       (long)Math.Round(Ticks / divisor, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Clones the current time span.
        /// </summary>
        /// <returns>Copy of the current time span.</returns>
        public ITimeSpan Clone()
        {
            return new BarBeatTimeSpan(Bars, Beats, Ticks);
        }

        #endregion
    }
}

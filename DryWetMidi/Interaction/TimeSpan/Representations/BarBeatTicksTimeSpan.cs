using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents bar/beat time span which represents bars, beats and ticks. More info in the
    /// <see href="xref:a_time_length#bars-beats-and-ticks">Time and length: Representations: Bars, beats and ticks</see> article.
    /// </summary>
    public sealed class BarBeatTicksTimeSpan : ITimeSpan, IComparable<BarBeatTicksTimeSpan>, IEquatable<BarBeatTicksTimeSpan>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTicksTimeSpan"/>.
        /// </summary>
        public BarBeatTicksTimeSpan()
            : this(0, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTicksTimeSpan"/> with the specified
        /// number of bars.
        /// </summary>
        /// <param name="bars">The number of bars.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bars"/> is negative.</exception>
        public BarBeatTicksTimeSpan(long bars)
            : this(bars, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTicksTimeSpan"/> with the specified
        /// number of bars and beats.
        /// </summary>
        /// <param name="bars">The number of bars.</param>
        /// <param name="beats">The number of beats.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="bars"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="beats"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public BarBeatTicksTimeSpan(long bars, long beats)
            : this(bars, beats, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarBeatTicksTimeSpan"/> with the specified
        /// number of bars, beats and ticks.
        /// </summary>
        /// <param name="bars">The number of bars.</param>
        /// <param name="beats">The number of beats.</param>
        /// <param name="ticks">The number of ticks.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="bars"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="beats"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="ticks"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public BarBeatTicksTimeSpan(long bars, long beats, long ticks)
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
        /// Gets the bars component of the time represented by the current <see cref="BarBeatTicksTimeSpan"/>.
        /// </summary>
        public long Bars { get; }

        /// <summary>
        /// Gets the beats component of the time represented by the current <see cref="BarBeatTicksTimeSpan"/>.
        /// </summary>
        public long Beats { get; }

        /// <summary>
        /// Gets the ticks component of the time represented by the current <see cref="BarBeatTicksTimeSpan"/>.
        /// </summary>
        public long Ticks { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a bar/beat time span to its <see cref="BarBeatTicksTimeSpan"/>
        /// equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="BarBeatTicksTimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out BarBeatTicksTimeSpan timeSpan)
        {
            return ParsingUtilities.TryParse(input, BarBeatTicksTimeSpanParser.TryParse, out timeSpan);
        }

        /// <summary>
        /// Converts the string representation of a bar/beat time span to its <see cref="BarBeatTicksTimeSpan"/>
        /// equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="BarBeatTicksTimeSpan"/> equivalent to the time span contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static BarBeatTicksTimeSpan Parse(string input)
        {
            return ParsingUtilities.Parse<BarBeatTicksTimeSpan>(input, BarBeatTicksTimeSpanParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="BarBeatTicksTimeSpan"/> objects are equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if time spans are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, null))
                return ReferenceEquals(timeSpan2, null);

            return timeSpan1.Equals(timeSpan2);
        }

        /// <summary>
        /// Determines if two <see cref="BarBeatTicksTimeSpan"/> objects are not equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>false</c> if time spans are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        /// <summary>
        /// Adds two specified <see cref="BarBeatTicksTimeSpan"/> instances.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to add.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to add.</param>
        /// <returns>An object whose value is the sum of the values of <paramref name="timeSpan1"/> and
        /// <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static BarBeatTicksTimeSpan operator +(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new BarBeatTicksTimeSpan(timeSpan1.Bars + timeSpan2.Bars,
                                       timeSpan1.Beats + timeSpan2.Beats,
                                       timeSpan1.Ticks + timeSpan2.Ticks);
        }

        /// <summary>
        /// Subtracts a specified <see cref="BarBeatTicksTimeSpan"/> from another one.
        /// </summary>
        /// <param name="timeSpan1">The minuend.</param>
        /// <param name="timeSpan2">The subtrahend.</param>
        /// <returns>An object whose value is the result of the value of <paramref name="timeSpan1"/> minus
        /// the value of <paramref name="timeSpan2"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="timeSpan1"/> is less than <paramref name="timeSpan2"/>.</exception>
        public static BarBeatTicksTimeSpan operator -(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new BarBeatTicksTimeSpan(timeSpan1.Bars - timeSpan2.Bars,
                                       timeSpan1.Beats - timeSpan2.Beats,
                                       timeSpan1.Ticks - timeSpan2.Ticks);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTicksTimeSpan"/> is less than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSpan1"/> is less than the value of
        /// <paramref name="timeSpan2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) < 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTicksTimeSpan"/> is greater than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSpan1"/> is greater than the value of
        /// <paramref name="timeSpan2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) > 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTicksTimeSpan"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSpan1"/> is less than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <=(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) <= 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="BarBeatTicksTimeSpan"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="BarBeatTicksTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="timeSpan1"/> is greater than or equal to the value of
        /// <paramref name="timeSpan2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSpan2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >=(BarBeatTicksTimeSpan timeSpan1, BarBeatTicksTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) >= 0;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BarBeatTicksTimeSpan);
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
                result = result * 23 + Bars.GetHashCode();
                result = result * 23 + Beats.GetHashCode();
                result = result * 23 + Ticks.GetHashCode();
                return result;
            }
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
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is invalid.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="mode"/> specified an invalid value.</exception>
        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            var barBeatTicksTimeSpan = timeSpan as BarBeatTicksTimeSpan;
            return barBeatTicksTimeSpan != null
                ? this + barBeatTicksTimeSpan
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
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is invalid.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="mode"/> specified an invalid value.</exception>
        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            var barBeatTicksTimeSpan = timeSpan as BarBeatTicksTimeSpan;
            return barBeatTicksTimeSpan != null
                ? this - barBeatTicksTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
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

            return new BarBeatTicksTimeSpan(MathUtilities.RoundToLong(Bars * multiplier),
                                       MathUtilities.RoundToLong(Beats * multiplier),
                                       MathUtilities.RoundToLong(Ticks * multiplier));
        }

        /// <summary>
        /// Shrinks the current time span by dividing its length by the specified divisor.
        /// </summary>
        /// <param name="divisor">Divisor to shrink the time span by.</param>
        /// <returns>Time span that is the current time span shrinked by the <paramref name="divisor"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is zero or negative.</exception>
        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new BarBeatTicksTimeSpan(MathUtilities.RoundToLong(Bars / divisor),
                                       MathUtilities.RoundToLong(Beats / divisor),
                                       MathUtilities.RoundToLong(Ticks / divisor));
        }

        /// <summary>
        /// Clones the current time span.
        /// </summary>
        /// <returns>Copy of the current time span.</returns>
        public ITimeSpan Clone()
        {
            return new BarBeatTicksTimeSpan(Bars, Beats, Ticks);
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
            if (ReferenceEquals(other, null))
                return 1;

            var barBeatTicksTimeSpan = other as BarBeatTicksTimeSpan;
            if (ReferenceEquals(barBeatTicksTimeSpan, null))
                throw new ArgumentException("Time span is of different type.", nameof(other));

            return CompareTo(barBeatTicksTimeSpan);
        }

        #endregion

        #region IComparable<BarBeatTicksTimeSpan>

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
        public int CompareTo(BarBeatTicksTimeSpan other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            var barsDelta = Bars - other.Bars;
            var beatsDelta = Beats - other.Beats;
            var ticksDelta = Ticks - other.Ticks;

            return Math.Sign(barsDelta != 0 ? barsDelta : (beatsDelta != 0 ? beatsDelta : ticksDelta));
        }

        #endregion

        #region IEquatable<BarBeatTicksTimeSpan>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(BarBeatTicksTimeSpan other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(null, other))
                return false;

            return Bars == other.Bars &&
                   Beats == other.Beats &&
                   Ticks == other.Ticks;
        }

        #endregion
    }
}

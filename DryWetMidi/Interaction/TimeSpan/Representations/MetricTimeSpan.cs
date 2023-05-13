using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents metric time span which represents hours, minutes and seconds. More info in the
    /// <see href="xref:a_time_length#metric">Time and length: Representations: Metric</see> article.
    /// </summary>
    public sealed class MetricTimeSpan : ITimeSpan, IComparable<MetricTimeSpan>, IEquatable<MetricTimeSpan>
    {
        #region Constants

        private const int MicrosecondsInMillisecond = 1000;
        private const long TicksInMicrosecond = TimeSpan.TicksPerMillisecond / MicrosecondsInMillisecond;
        private const long MaxTotalMicroseconds = long.MaxValue / TicksInMicrosecond;

        private static readonly string TotalMicrosecondsIsOutOfRangeMessage =
            $"Number of microseconds is out of [{0};{MaxTotalMicroseconds}] range.";

        #endregion

        #region Fields

        private readonly TimeSpan _timeSpan;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/>.
        /// </summary>
        public MetricTimeSpan()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// number of microseconds.
        /// </summary>
        /// <param name="totalMicroseconds">Number of microseconds which represents metric time span.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalMicroseconds"/> is negative.</exception>
        public MetricTimeSpan(long totalMicroseconds)
        {
            ThrowIfArgument.IsOutOfRange(
                nameof(totalMicroseconds),
                totalMicroseconds,
                0,
                MaxTotalMicroseconds,
                TotalMicrosecondsIsOutOfRangeMessage);

            _timeSpan = new TimeSpan(totalMicroseconds * TicksInMicrosecond);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="timeSpan">Time interval to initialize the <see cref="MetricTimeSpan"/>.</param>
        public MetricTimeSpan(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// numbers of hours, minutes and seconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="hours"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="minutes"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="seconds"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public MetricTimeSpan(int hours, int minutes, int seconds)
            : this(hours, minutes, seconds, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricTimeSpan"/> with the specified
        /// numbers of hours, minutes, seconds and milliseconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="milliseconds">Number of milliseconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="hours"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="minutes"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="seconds"/> is negative.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="milliseconds"/> is negative.</description>
        /// </item>
        /// </list>
        /// </exception>
        public MetricTimeSpan(int hours, int minutes, int seconds, int milliseconds)
        {
            ThrowIfArgument.IsNegative(nameof(hours), hours, "Number of hours is negative.");
            ThrowIfArgument.IsNegative(nameof(minutes), minutes, "Number of minutes is negative.");
            ThrowIfArgument.IsNegative(nameof(seconds), seconds, "Number of seconds is negative.");
            ThrowIfArgument.IsNegative(nameof(milliseconds), milliseconds, "Number of milliseconds is negative.");

            _timeSpan = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in microseconds.
        /// </summary>
        public long TotalMicroseconds => _timeSpan.Ticks / TicksInMicrosecond;

        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in whole and fractional milliseconds.
        /// </summary>
        public double TotalMilliseconds => _timeSpan.TotalMilliseconds;

        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in whole and fractional seconds.
        /// </summary>
        public double TotalSeconds => _timeSpan.TotalSeconds;

        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in whole and fractional minutes.
        /// </summary>
        public double TotalMinutes => _timeSpan.TotalMinutes;

        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in whole and fractional hours.
        /// </summary>
        public double TotalHours => _timeSpan.TotalHours;
        
        /// <summary>
        /// Gets the value of the current <see cref="MetricTimeSpan"/> expressed in whole and fractional days.
        /// </summary>
        public double TotalDays => _timeSpan.TotalDays;

        /// <summary>
        /// Gets the hours component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Hours => _timeSpan.Hours;

        /// <summary>
        /// Gets the minutes component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Minutes => _timeSpan.Minutes;

        /// <summary>
        /// Gets the seconds component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Seconds => _timeSpan.Seconds;

        /// <summary>
        /// Gets the milliseconds component of the time represented by the current <see cref="MetricTimeSpan"/>.
        /// </summary>
        public int Milliseconds => _timeSpan.Milliseconds;

        #endregion

        #region Methods

        /// <summary>
        /// Divides the current time span by the specified <see cref="MetricTimeSpan"/> returning ration
        /// between them.
        /// </summary>
        /// <param name="timeSpan"><see cref="MetricTimeSpan"/> to divide the current time span by.</param>
        /// <returns>Ratio of the current <see cref="MetricTimeSpan"/> and <paramref name="timeSpan"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is <c>null</c>.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="timeSpan"/> represents a time span of zero length.</exception>
        public double Divide(MetricTimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var ticks = timeSpan._timeSpan.Ticks;
            if (ticks == 0)
                throw new DivideByZeroException("Dividing by zero time span.");

            return (double)_timeSpan.Ticks / timeSpan._timeSpan.Ticks;
        }

        /// <summary>
        /// Converts the string representation of a metric time span to its <see cref="MetricTimeSpan"/>
        /// equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="MetricTimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out MetricTimeSpan timeSpan)
        {
            return ParsingUtilities.TryParse(input, MetricTimeSpanParser.TryParse, out timeSpan);
        }

        /// <summary>
        /// Converts the string representation of a metric time span to its <see cref="MetricTimeSpan"/>
        /// equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="MetricTimeSpan"/> equivalent to the time span contained in
        /// <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static MetricTimeSpan Parse(string input)
        {
            return ParsingUtilities.Parse<MetricTimeSpan>(input, MetricTimeSpanParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts <see cref="TimeSpan"/> to <see cref="MetricTimeSpan"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="TimeSpan"/> to convert to <see cref="MetricTimeSpan"/>.</param>
        /// <returns><paramref name="timeSpan"/> represented as <see cref="MetricTimeSpan"/>.</returns>
        public static implicit operator MetricTimeSpan(TimeSpan timeSpan)
        {
            return new MetricTimeSpan(timeSpan);
        }

        /// <summary>
        /// Converts <see cref="MetricTimeSpan"/> to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timeSpan"><see cref="MetricTimeSpan"/> to convert to <see cref="TimeSpan"/>.</param>
        /// <returns><paramref name="timeSpan"/> represented as <see cref="TimeSpan"/>.</returns>
        public static implicit operator TimeSpan(MetricTimeSpan timeSpan)
        {
            return timeSpan._timeSpan;
        }

        /// <summary>
        /// Determines if two <see cref="MetricTimeSpan"/> objects are equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns><c>true</c> if time spans are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, null))
                return ReferenceEquals(timeSpan2, null);

            return timeSpan1.Equals(timeSpan2);
        }

        /// <summary>
        /// Determines if two <see cref="MetricTimeSpan"/> objects are not equal.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
        /// <returns><c>false</c> if time spans are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        /// <summary>
        /// Adds two specified <see cref="MetricTimeSpan"/> instances.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to add.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to add.</param>
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
        public static MetricTimeSpan operator +(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds + timeSpan2.TotalMicroseconds);
        }

        /// <summary>
        /// Subtracts a specified <see cref="MetricTimeSpan"/> from another one.
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
        public static MetricTimeSpan operator -(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MetricTimeSpan(timeSpan1.TotalMicroseconds - timeSpan2.TotalMicroseconds);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is less than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
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
        public static bool operator <(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) < 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is greater than another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
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
        public static bool operator >(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) > 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
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
        public static bool operator <=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) <= 0;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="MetricTimeSpan"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="timeSpan1">The first <see cref="MetricTimeSpan"/> to compare.</param>
        /// <param name="timeSpan2">The second <see cref="MetricTimeSpan"/> to compare.</param>
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
        public static bool operator >=(MetricTimeSpan timeSpan1, MetricTimeSpan timeSpan2)
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
            return Equals(obj as MetricTimeSpan);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return TotalMicroseconds.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Hours}:{Minutes}:{Seconds}:{Milliseconds}";
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

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this + metricTimeSpan
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

            var metricTimeSpan = timeSpan as MetricTimeSpan;
            return metricTimeSpan != null
                ? this - metricTimeSpan
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

            return new MetricTimeSpan(MathUtilities.RoundToLong(TotalMicroseconds * multiplier));
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

            return new MetricTimeSpan(MathUtilities.RoundToLong(TotalMicroseconds / divisor));
        }

        /// <summary>
        /// Clones the current time span.
        /// </summary>
        /// <returns>Copy of the current time span.</returns>
        public ITimeSpan Clone()
        {
            return new MetricTimeSpan(_timeSpan);
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

            var metricTimeSpan = other as MetricTimeSpan;
            if (ReferenceEquals(metricTimeSpan, null))
                throw new ArgumentException("Time span is of different type.", nameof(other));

            return CompareTo(metricTimeSpan);
        }

        #endregion

        #region IComparable<MetricTimeSpan>

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
        public int CompareTo(MetricTimeSpan other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            return _timeSpan.CompareTo(other._timeSpan);
        }

        #endregion

        #region IEquatable<MetricTimeSpan>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(MetricTimeSpan other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(null, other))
                return false;

            return _timeSpan == other._timeSpan;
        }

        #endregion
    }
}

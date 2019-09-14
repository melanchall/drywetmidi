using System;
using System.Globalization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class BarBeatCentsTimeSpan : ITimeSpan, IComparable<BarBeatCentsTimeSpan>, IEquatable<BarBeatCentsTimeSpan>
    {
        #region Constants

        public const double MaxCents = 100.0 - CentsEpsilon;

        internal const double CentsEpsilon = 0.00001;

        #endregion

        #region Constructor

        public BarBeatCentsTimeSpan()
            : this(0, 0)
        {
        }

        public BarBeatCentsTimeSpan(long bars)
            : this(bars, 0)
        {
        }

        public BarBeatCentsTimeSpan(long bars, long beats)
            : this(bars, beats, 0)
        {
        }

        public BarBeatCentsTimeSpan(long bars, long beats, double cents)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNegative(nameof(beats), beats, "Beats number is negative.");
            ThrowIfArgument.IsOutOfRange(nameof(cents), cents, 0, MaxCents, $"Cents number is out of [0, {MaxCents}] range.");

            Bars = bars;
            Beats = beats;
            Cents = cents;
        }

        #endregion

        #region Properties

        public long Bars { get; }

        public long Beats { get; }

        public double Cents { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out BarBeatCentsTimeSpan timeSpan)
        {
            return ParsingUtilities.TryParse(input, BarBeatCentsTimeSpanParser.TryParse, out timeSpan);
        }

        public static BarBeatCentsTimeSpan Parse(string input)
        {
            return ParsingUtilities.Parse<BarBeatCentsTimeSpan>(input, BarBeatCentsTimeSpanParser.TryParse);
        }

        #endregion

        #region Operators

        public static bool operator ==(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, null))
                return ReferenceEquals(timeSpan2, null);

            return timeSpan1.Equals(timeSpan2);
        }

        public static bool operator !=(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static BarBeatCentsTimeSpan operator +(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new BarBeatCentsTimeSpan(timeSpan1.Bars + timeSpan2.Bars,
                                            timeSpan1.Beats + timeSpan2.Beats,
                                            timeSpan1.Cents + timeSpan2.Cents);
        }

        public static BarBeatCentsTimeSpan operator -(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new BarBeatCentsTimeSpan(timeSpan1.Bars - timeSpan2.Bars,
                                            timeSpan1.Beats - timeSpan2.Beats,
                                            timeSpan1.Cents - timeSpan2.Cents);
        }

        public static bool operator <(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) < 0;
        }

        public static bool operator >(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) > 0;
        }

        public static bool operator <=(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) <= 0;
        }

        public static bool operator >=(BarBeatCentsTimeSpan timeSpan1, BarBeatCentsTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) >= 0;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as BarBeatCentsTimeSpan);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Bars.GetHashCode();
                result = result * 23 + Beats.GetHashCode();
                result = result * 23 + Cents.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            var centsPart = Cents.ToString(CultureInfo.InvariantCulture);
            if (centsPart.IndexOf('.') < 0)
                centsPart += ".0";

            return $"{Bars}.{Beats}.{centsPart}";
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatCentsTimeSpan = timeSpan as BarBeatCentsTimeSpan;
            return barBeatCentsTimeSpan != null
                ? this + barBeatCentsTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatCentsTimeSpan = timeSpan as BarBeatCentsTimeSpan;
            return barBeatCentsTimeSpan != null
                ? this - barBeatCentsTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new BarBeatCentsTimeSpan(MathUtilities.RoundToLong(Bars * multiplier),
                                            MathUtilities.RoundToLong(Beats * multiplier),
                                            MathUtilities.RoundToLong(Cents * multiplier));
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new BarBeatCentsTimeSpan(MathUtilities.RoundToLong(Bars / divisor),
                                            MathUtilities.RoundToLong(Beats / divisor),
                                            MathUtilities.RoundToLong(Cents / divisor));
        }

        public ITimeSpan Clone()
        {
            return new BarBeatCentsTimeSpan(Bars, Beats, Cents);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null))
                return 1;

            var barBeatCentsTimeSpan = obj as BarBeatCentsTimeSpan;
            if (ReferenceEquals(barBeatCentsTimeSpan, null))
                throw new ArgumentException("Time span is of different type.", nameof(obj));

            return CompareTo(barBeatCentsTimeSpan);
        }

        #endregion

        #region IComparable<BarBeatCentsTimeSpan>

        public int CompareTo(BarBeatCentsTimeSpan other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            var barsDelta = Bars - other.Bars;
            var beatsDelta = Beats - other.Beats;
            var centsDelta = Cents - other.Cents;

            return Math.Sign(barsDelta != 0 ? barsDelta : (beatsDelta != 0 ? beatsDelta : centsDelta));
        }

        #endregion

        #region IEquatable<BarBeatCentsTimeSpan>

        public bool Equals(BarBeatCentsTimeSpan other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(null, other))
                return false;

            return Bars == other.Bars &&
                   Beats == other.Beats &&
                   Math.Abs(Cents - other.Cents) < CentsEpsilon;
        }

        #endregion
    }
}

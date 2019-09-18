using System;
using System.Globalization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class BarBeatFractionTimeSpan : ITimeSpan, IComparable<BarBeatFractionTimeSpan>, IEquatable<BarBeatFractionTimeSpan>
    {
        #region Constructor

        public BarBeatFractionTimeSpan()
            : this(0, 0)
        {
        }

        public BarBeatFractionTimeSpan(long bars)
            : this(bars, 0)
        {
        }

        public BarBeatFractionTimeSpan(long bars, double beats)
        {
            ThrowIfArgument.IsNegative(nameof(bars), bars, "Bars number is negative.");
            ThrowIfArgument.IsNegative(nameof(beats), beats, "Beats number is negative.");

            Bars = bars;
            Beats = beats;
        }

        #endregion

        #region Properties

        public long Bars { get; }

        public double Beats { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out BarBeatFractionTimeSpan timeSpan)
        {
            return ParsingUtilities.TryParse(input, BarBeatFractionTimeSpanParser.TryParse, out timeSpan);
        }

        public static BarBeatFractionTimeSpan Parse(string input)
        {
            return ParsingUtilities.Parse<BarBeatFractionTimeSpan>(input, BarBeatFractionTimeSpanParser.TryParse);
        }

        #endregion

        #region Operators

        public static bool operator ==(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, null))
                return ReferenceEquals(timeSpan2, null);

            return timeSpan1.Equals(timeSpan2);
        }

        public static bool operator !=(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static BarBeatFractionTimeSpan operator +(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new BarBeatFractionTimeSpan(timeSpan1.Bars + timeSpan2.Bars,
                                               timeSpan1.Beats + timeSpan2.Beats);
        }

        public static BarBeatFractionTimeSpan operator -(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1 < timeSpan2)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new BarBeatFractionTimeSpan(timeSpan1.Bars - timeSpan2.Bars,
                                               timeSpan1.Beats - timeSpan2.Beats);
        }

        public static bool operator <(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) < 0;
        }

        public static bool operator >(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) > 0;
        }

        public static bool operator <=(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) <= 0;
        }

        public static bool operator >=(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.CompareTo(timeSpan2) >= 0;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as BarBeatFractionTimeSpan);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Bars.GetHashCode();
                result = result * 23 + Beats.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return $"{Bars}_{Beats.ToString(CultureInfo.InvariantCulture)}";
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatFractionTimeSpan = timeSpan as BarBeatFractionTimeSpan;
            return barBeatFractionTimeSpan != null
                ? this + barBeatFractionTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatFractionTimeSpan = timeSpan as BarBeatFractionTimeSpan;
            return barBeatFractionTimeSpan != null
                ? this - barBeatFractionTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new BarBeatFractionTimeSpan(MathUtilities.RoundToLong(Bars * multiplier),
                                               Beats * multiplier);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new BarBeatFractionTimeSpan(MathUtilities.RoundToLong(Bars / divisor),
                                               Beats / divisor);
        }

        public ITimeSpan Clone()
        {
            return new BarBeatFractionTimeSpan(Bars, Beats);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null))
                return 1;

            var barBeatFractionTimeSpan = obj as BarBeatFractionTimeSpan;
            if (ReferenceEquals(barBeatFractionTimeSpan, null))
                throw new ArgumentException("Time span is of different type.", nameof(obj));

            return CompareTo(barBeatFractionTimeSpan);
        }

        #endregion

        #region IComparable<BarBeatFractionTimeSpan>

        public int CompareTo(BarBeatFractionTimeSpan other)
        {
            if (ReferenceEquals(other, null))
                return 1;

            var barsDelta = Bars - other.Bars;
            var beatsDelta = Beats - other.Beats;

            return Math.Sign(barsDelta != 0 ? barsDelta : beatsDelta);
        }

        #endregion

        #region IEquatable<BarBeatFractionTimeSpan>

        public bool Equals(BarBeatFractionTimeSpan other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(null, other))
                return false;

            return Bars == other.Bars &&
                   Beats == other.Beats;
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class BarBeatTimeSpan : ITimeSpan
    {
        #region Constructor

        public BarBeatTimeSpan()
            : this(0, 0)
        {
        }

        public BarBeatTimeSpan(long bars, long beats)
            : this(bars, beats, 0)
        {
        }

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

        public long Bars { get; }

        public long Beats { get; }

        public long Ticks { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out BarBeatTimeSpan timeSpan)
        {
            return BarBeatTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static BarBeatTimeSpan Parse(string input)
        {
            var parsingResult = BarBeatTimeSpanParser.TryParse(input, out var timeSpan);
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

        public static bool operator !=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static BarBeatTimeSpan operator +(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new BarBeatTimeSpan(timeSpan1.Bars + timeSpan2.Bars,
                                       timeSpan1.Beats + timeSpan2.Beats,
                                       timeSpan1.Ticks + timeSpan2.Ticks);
        }

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

        public static bool operator <(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            CalculateDifferencies(timeSpan1, timeSpan2, out var barsDelta, out var beatsDelta, out var ticksDelta);
            return barsDelta < 0 || (barsDelta == 0 && (beatsDelta < 0 || (beatsDelta == 0 && ticksDelta < 0)));
        }

        public static bool operator >(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            CalculateDifferencies(timeSpan1, timeSpan2, out var barsDelta, out var beatsDelta, out var ticksDelta);
            return barsDelta > 0 || (barsDelta == 0 && (beatsDelta > 0 || (beatsDelta == 0 && ticksDelta > 0)));
        }

        public static bool operator <=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            CalculateDifferencies(timeSpan1, timeSpan2, out var barsDelta, out var beatsDelta, out var ticksDelta);
            return barsDelta < 0 || (barsDelta == 0 && (beatsDelta < 0 || (beatsDelta == 0 && ticksDelta <= 0)));
        }

        public static bool operator >=(BarBeatTimeSpan timeSpan1, BarBeatTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            CalculateDifferencies(timeSpan1, timeSpan2, out var barsDelta, out var beatsDelta, out var ticksDelta);
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

        public ITimeSpan Add(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatTimeSpan = timeSpan as BarBeatTimeSpan;
            return barBeatTimeSpan != null
                ? this + barBeatTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, operationMode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var barBeatTimeSpan = timeSpan as BarBeatTimeSpan;
            return barBeatTimeSpan != null
                ? this - barBeatTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, operationMode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new BarBeatTimeSpan((long)Math.Round(Bars * multiplier),
                                       (long)Math.Round(Beats * multiplier),
                                       (long)Math.Round(Ticks * multiplier));
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new BarBeatTimeSpan((long)Math.Round(Bars / divisor),
                                       (long)Math.Round(Beats / divisor),
                                       (long)Math.Round(Ticks / divisor));
        }

        #endregion
    }
}

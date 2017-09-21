using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MidiTimeSpan : ITimeSpan
    {
        #region Constructor

        public MidiTimeSpan()
            : this(0)
        {
        }

        public MidiTimeSpan(long timeSpan)
        {
            ThrowIfLengthArgument.IsNegative(nameof(timeSpan), timeSpan);

            TimeSpan = timeSpan;
        }

        #endregion

        #region Properties

        public long TimeSpan { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MidiTimeSpan timeSpan)
        {
            return MidiTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static MidiTimeSpan Parse(string input)
        {
            var parsingResult = MidiTimeSpanParser.TryParse(input, out var timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static explicit operator MidiTimeSpan(long timeSpan)
        {
            return new MidiTimeSpan(timeSpan);
        }

        public static implicit operator long(MidiTimeSpan timeSpan)
        {
            return timeSpan.TimeSpan;
        }

        public static bool operator ==(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.TimeSpan == timeSpan2.TimeSpan;
        }

        public static bool operator !=(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        public static MidiTimeSpan operator +(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return new MidiTimeSpan(timeSpan1.TimeSpan + timeSpan2.TimeSpan);
        }

        public static MidiTimeSpan operator -(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            if (timeSpan1.TimeSpan < timeSpan2.TimeSpan)
                throw new ArgumentException("First time span is less than second one.", nameof(timeSpan1));

            return new MidiTimeSpan(timeSpan1.TimeSpan - timeSpan2.TimeSpan);
        }

        public static bool operator <(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TimeSpan < timeSpan2.TimeSpan;
        }

        public static bool operator >(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TimeSpan > timeSpan2.TimeSpan;
        }

        public static bool operator <=(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TimeSpan <= timeSpan2.TimeSpan;
        }

        public static bool operator >=(MidiTimeSpan timeSpan1, MidiTimeSpan timeSpan2)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);

            return timeSpan1.TimeSpan >= timeSpan2.TimeSpan;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return TimeSpan.ToString();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MidiTimeSpan);
        }

        public override int GetHashCode()
        {
            return TimeSpan.GetHashCode();
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var midiTimeSpan = timeSpan as MidiTimeSpan;
            return midiTimeSpan != null
                ? this + midiTimeSpan
                : TimeSpanUtilities.Add(this, timeSpan, operationMode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var midiTimeSpan = timeSpan as MidiTimeSpan;
            return midiTimeSpan != null
                ? this - midiTimeSpan
                : TimeSpanUtilities.Subtract(this, timeSpan, operationMode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MidiTimeSpan((long)Math.Round(TimeSpan * multiplier));
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNonpositive(nameof(divisor), divisor, "Divisor is zero or negative.");

            return new MidiTimeSpan((long)Math.Round(TimeSpan / divisor));
        }

        #endregion
    }
}

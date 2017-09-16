using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MathTimeSpan : ITimeSpan
    {
        #region Constructor

        public MathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation = default(MathOperation))
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);

            TimeSpan1 = timeSpan1;
            TimeSpan2 = timeSpan2;
            Operation = operation;
        }

        #endregion

        #region Properties

        public ITimeSpan TimeSpan1 { get; }

        public ITimeSpan TimeSpan2 { get; }

        public MathOperation Operation { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MathTimeSpan timeSpan)
        {
            return MathTimeSpanParser.TryParse(input, out timeSpan).Status == ParsingStatus.Parsed;
        }

        public static MathTimeSpan Parse(string input)
        {
            var parsingResult = MathTimeSpanParser.TryParse(input, out var timeSpan);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return timeSpan;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static bool operator ==(MathTimeSpan timeSpan1, MathTimeSpan timeSpan2)
        {
            if (ReferenceEquals(timeSpan1, timeSpan2))
                return true;

            if (ReferenceEquals(null, timeSpan1) || ReferenceEquals(null, timeSpan2))
                return false;

            return timeSpan1.TimeSpan1.Equals(timeSpan2.TimeSpan1) &&
                   timeSpan1.TimeSpan2.Equals(timeSpan2.TimeSpan2) &&
                   timeSpan1.Operation == timeSpan2.Operation;
        }

        public static bool operator !=(MathTimeSpan timeSpan1, MathTimeSpan timeSpan2)
        {
            return !(timeSpan1 == timeSpan2);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            var operationString = Operation == MathOperation.Add
                ? "+"
                : "-";

            return $"({TimeSpan1} {operationString} {TimeSpan2})";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MathTimeSpan);
        }

        public override int GetHashCode()
        {
            return TimeSpan1.GetHashCode() ^ Operation.GetHashCode() ^ TimeSpan2.GetHashCode();
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Add(this, timeSpan);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Subtract(this, timeSpan);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathTimeSpan(TimeSpan1.Multiply(multiplier),
                                    TimeSpan2.Multiply(multiplier),
                                    Operation);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MathTimeSpan(TimeSpan1.Divide(divisor),
                                    TimeSpan2.Divide(divisor),
                                    Operation);
        }

        #endregion
    }
}

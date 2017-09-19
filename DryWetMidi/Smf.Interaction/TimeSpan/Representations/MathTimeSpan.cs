using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MathTimeSpan : ITimeSpan
    {
        #region Constants

        private const string TimeModeString = "T";
        private const string LengthModeString = "L";

        private static readonly Dictionary<MathOperationMode, Tuple<string, string>> ModeStrings =
            new Dictionary<MathOperationMode, Tuple<string, string>>
            {
                [MathOperationMode.Unspecified] = Tuple.Create(string.Empty, string.Empty),
                [MathOperationMode.TimeTime] = Tuple.Create(TimeModeString, TimeModeString),
                [MathOperationMode.TimeLength] = Tuple.Create(TimeModeString, LengthModeString),
                [MathOperationMode.LengthLength] = Tuple.Create(LengthModeString, LengthModeString),
            };

        #endregion

        #region Constructor

        public MathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operationMode), operationMode);

            TimeSpan1 = timeSpan1;
            TimeSpan2 = timeSpan2;
            Operation = operation;
            OperationMode = operationMode;
        }

        #endregion

        #region Properties

        public ITimeSpan TimeSpan1 { get; }

        public ITimeSpan TimeSpan2 { get; }

        public MathOperation Operation { get; }

        public MathOperationMode OperationMode { get; }

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
                   timeSpan1.Operation == timeSpan2.Operation &&
                   timeSpan1.OperationMode == timeSpan2.OperationMode;
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

            var modeStrings = ModeStrings[OperationMode];

            return $"({TimeSpan1}{modeStrings.Item1} {operationString} {TimeSpan2}{modeStrings.Item2})";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MathTimeSpan);
        }

        public override int GetHashCode()
        {
            return TimeSpan1.GetHashCode() ^
                   TimeSpan2.GetHashCode() ^
                   Operation.GetHashCode() ^
                   OperationMode.GetHashCode();
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Add(this, timeSpan, operationMode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, MathOperationMode operationMode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Subtract(this, timeSpan, operationMode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathTimeSpan(TimeSpan1.Multiply(multiplier),
                                    TimeSpan2.Multiply(multiplier),
                                    Operation,
                                    OperationMode);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MathTimeSpan(TimeSpan1.Divide(divisor),
                                    TimeSpan2.Divide(divisor),
                                    Operation,
                                    OperationMode);
        }

        #endregion
    }
}

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

        private static readonly Dictionary<TimeSpanMode, Tuple<string, string>> ModeStrings =
            new Dictionary<TimeSpanMode, Tuple<string, string>>
            {
                [TimeSpanMode.TimeTime] = Tuple.Create(TimeModeString, TimeModeString),
                [TimeSpanMode.TimeLength] = Tuple.Create(TimeModeString, LengthModeString),
                [TimeSpanMode.LengthLength] = Tuple.Create(LengthModeString, LengthModeString),
            };

        #endregion

        #region Constructor

        public MathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2, MathOperation operation, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan1), timeSpan1);
            ThrowIfArgument.IsNull(nameof(timeSpan2), timeSpan2);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            TimeSpan1 = timeSpan1;
            TimeSpan2 = timeSpan2;
            Operation = operation;
            Mode = mode;
        }

        #endregion

        #region Properties

        public ITimeSpan TimeSpan1 { get; }

        public ITimeSpan TimeSpan2 { get; }

        public MathOperation Operation { get; }

        public TimeSpanMode Mode { get; }

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
                   timeSpan1.Mode == timeSpan2.Mode;
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

            var modeStrings = ModeStrings[Mode];

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
                   Mode.GetHashCode();
        }

        #endregion

        #region ITimeSpan

        public ITimeSpan Add(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Add(this, timeSpan, mode);
        }

        public ITimeSpan Subtract(ITimeSpan timeSpan, TimeSpanMode mode)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            return TimeSpanUtilities.Subtract(this, timeSpan, mode);
        }

        public ITimeSpan Multiply(double multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathTimeSpan(TimeSpan1.Multiply(multiplier),
                                    TimeSpan2.Multiply(multiplier),
                                    Operation,
                                    Mode);
        }

        public ITimeSpan Divide(double divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MathTimeSpan(TimeSpan1.Divide(divisor),
                                    TimeSpan2.Divide(divisor),
                                    Operation,
                                    Mode);
        }

        #endregion
    }
}

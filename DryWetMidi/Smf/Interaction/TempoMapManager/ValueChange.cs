using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ValueChange<T> : ITimedObject
    {
        #region Constructor

        public ValueChange(long time, T value)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Time = time;
            Value = value;
        }

        #endregion

        #region Properties

        public long Time { get; }

        public T Value { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Value} at {Time}";
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class MidiTime : ITime
    {
        #region Constructor

        public MidiTime(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            Time = time;
        }

        #endregion

        #region Properties

        public long Time { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MidiTime time)
        {
            return MidiTimeParser.TryParse(input, out time).Status == ParsingStatus.Parsed;
        }

        public static MidiTime Parse(string input)
        {
            var parsingResult = MidiTimeParser.TryParse(input, out var time);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return time;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static explicit operator MidiTime(long time)
        {
            return new MidiTime(time);
        }

        public static implicit operator long(MidiTime time)
        {
            return time.Time;
        }

        public static bool operator ==(MidiTime time1, MidiTime time2)
        {
            if (ReferenceEquals(time1, time2))
                return true;

            if (ReferenceEquals(null, time1) || ReferenceEquals(null, time2))
                return false;

            return time1.Time == time2.Time;
        }

        public static bool operator !=(MidiTime time1, MidiTime time2)
        {
            return !(time1 == time2);
        }

        public static MidiTime operator +(MidiTime time, MidiLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MidiTime(time.Time + length.Length);
        }

        public static MidiLength operator -(MidiTime time1, MidiTime time2)
        {
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            if (time1 < time2)
                throw new ArgumentException("First time is less than second one.", nameof(time1));

            return new MidiLength(time1.Time - time2.Time);
        }

        public static MidiTime operator -(MidiTime time, MidiLength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            if (time.Time < length.Length)
                throw new ArgumentException("Time is less than length.", nameof(time));

            return new MidiTime(time.Time - length.Length);
        }

        public static bool operator <(MidiTime time1, MidiTime time2)
        {
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return time1.Time < time2.Time;
        }

        public static bool operator >(MidiTime time1, MidiTime time2)
        {
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return time1.Time > time2.Time;
        }

        public static bool operator <=(MidiTime time1, MidiTime time2)
        {
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return time1.Time <= time2.Time;
        }

        public static bool operator >=(MidiTime time1, MidiTime time2)
        {
            ThrowIfArgument.IsNull(nameof(time1), time1);
            ThrowIfArgument.IsNull(nameof(time2), time2);

            return time1.Time >= time2.Time;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Time.ToString();
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MidiTime);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }

        #endregion
    }
}

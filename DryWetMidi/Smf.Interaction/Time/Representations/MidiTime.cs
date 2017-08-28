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

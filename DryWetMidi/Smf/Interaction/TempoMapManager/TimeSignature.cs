using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TimeSignature
    {
        #region Constants

        public static readonly TimeSignature Default = new TimeSignature(TimeSignatureEvent.DefaultNumerator,
                                                                         TimeSignatureEvent.DefaultDenominator);

        #endregion

        #region Constructor

        public TimeSignature(int numerator, int denominator)
        {
            if (numerator <= 0)
                throw new ArgumentException("Numerator is zero or negative.", nameof(numerator));

            if (denominator <= 0)
                throw new ArgumentException("Denominator is zero or negative.", nameof(denominator));

            if (!NumberUtilities.IsPowerOfTwo(denominator))
                throw new ArgumentException("Denominator is not a power of two.", nameof(denominator));

            Numerator = numerator;
            Denominator = denominator;
        }

        #endregion

        #region Properties

        public int Numerator { get; }

        public int Denominator { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var timeSignature = obj as TimeSignature;
            if (ReferenceEquals(null, timeSignature))
                return false;

            return Numerator == timeSignature.Numerator &&
                   Denominator == timeSignature.Denominator;
        }

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        #endregion
    }
}

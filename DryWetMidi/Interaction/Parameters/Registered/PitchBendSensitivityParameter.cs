using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class PitchBendSensitivityParameter : RegisteredParameter
    {
        #region Constants

        public static readonly SevenBitNumber DefaultHalfSteps = (SevenBitNumber)2;
        public static readonly SevenBitNumber DefaultCents = (SevenBitNumber)0;

        #endregion

        #region Constructor

        public PitchBendSensitivityParameter()
            : base(RegisteredParameterType.PitchBendSensitivity)
        {
        }

        public PitchBendSensitivityParameter(SevenBitNumber halfSteps, SevenBitNumber cents)
            : this(halfSteps, cents, ParameterValueType.Exact)
        {
        }

        public PitchBendSensitivityParameter(SevenBitNumber halfSteps, SevenBitNumber cents, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        public SevenBitNumber HalfSteps { get; set; } = DefaultHalfSteps;

        public SevenBitNumber Cents { get; set; } = DefaultCents;

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = HalfSteps;
            lsb = Cents;
        }

        protected override int GetIncrementStepsCount()
        {
            return HalfSteps * 100 + Cents;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps, {Cents} cents";
        }

        #endregion
    }
}

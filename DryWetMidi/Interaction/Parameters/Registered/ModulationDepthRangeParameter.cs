using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ModulationDepthRangeParameter : RegisteredParameter
    {
        #region Constants

        public static readonly SevenBitNumber DefaultHalfSteps = (SevenBitNumber)0;
        public static readonly float DefaultCents = 50f;

        public const float MinCents = 0f;
        public const float MaxCents = 100f;

        private const float CentResolution = 128 / 100f;

        #endregion

        #region Fields

        private float _cents = DefaultCents;

        #endregion

        #region Constructor

        public ModulationDepthRangeParameter()
            : base(RegisteredParameterType.ModulationDepthRange)
        {
        }

        public ModulationDepthRangeParameter(SevenBitNumber halfSteps, float cents)
            : this(halfSteps, cents, ParameterValueType.Exact)
        {
        }

        public ModulationDepthRangeParameter(SevenBitNumber halfSteps, float cents, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        public SevenBitNumber HalfSteps { get; set; } = DefaultHalfSteps;

        public float Cents
        {
            get { return _cents; }
            set
            {
                ThrowIfArgument.IsOutOfRange(
                    nameof(value),
                    value,
                    MinCents,
                    MaxCents,
                    $"Cents number is out of [{MinCents}; {MaxCents}] range.");

                _cents = value;
            }
        }

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = HalfSteps;
            lsb = (SevenBitNumber)MathUtilities.EnsureInBounds((int)Math.Round(Cents * CentResolution), SevenBitNumber.MinValue, SevenBitNumber.MaxValue);
        }

        protected override int GetIncrementStepsCount()
        {
            // TODO: find what is increment
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps, {Cents} cents";
        }

        #endregion
    }
}

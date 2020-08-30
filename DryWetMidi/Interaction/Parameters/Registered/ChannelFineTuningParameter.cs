using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChannelFineTuningParameter : RegisteredParameter
    {
        #region Constants

        public const float MinCents = -100f;
        public const float MaxCents = 100f;

        private const float CentResolution = 16383 / 200f;

        #endregion

        #region Fields

        private float _cents;

        #endregion

        #region Constructor

        public ChannelFineTuningParameter()
            : base(RegisteredParameterType.ChannelFineTuning)
        {
        }

        public ChannelFineTuningParameter(float cents)
            : this(cents, ParameterValueType.Exact)
        {
        }

        public ChannelFineTuningParameter(float cents, ParameterValueType valueType)
            : this()
        {
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

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

        #region Methods

        private int GetSteps()
        {
            // TODO: const
            return MathUtilities.EnsureInBounds((int)Math.Round((Cents + 100) * CentResolution), 0, 16383);
        }

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            var data = (ushort)GetSteps();
            msb = data.GetHead();
            lsb = data.GetTail();
        }

        protected override int GetIncrementStepsCount()
        {
            return GetSteps();
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {Cents} cents";
        }

        #endregion
    }
}

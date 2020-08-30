using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChannelCoarseTuningParameter : RegisteredParameter
    {
        #region Constants

        public const sbyte MinHalfSteps = -64;
        public const sbyte MaxHalfSteps = 63;

        #endregion

        #region Fields

        private sbyte _halfSteps;

        #endregion

        #region Constructor

        public ChannelCoarseTuningParameter()
            : base(RegisteredParameterType.ChannelCoarseTuning)
        {
        }

        public ChannelCoarseTuningParameter(sbyte halfSteps)
            : this(halfSteps, ParameterValueType.Exact)
        {
        }

        public ChannelCoarseTuningParameter(sbyte halfSteps, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        public sbyte HalfSteps
        {
            get { return _halfSteps; }
            set
            {
                ThrowIfArgument.IsOutOfRange(
                    nameof(value),
                    value,
                    MinHalfSteps,
                    MaxHalfSteps,
                    $"Half-steps number is out of [{MinHalfSteps}; {MaxHalfSteps}] range.");

                _halfSteps = value;
            }
        }

        #endregion

        #region Methods

        private int GetSteps()
        {
            return HalfSteps - MinHalfSteps;
        }

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            var data = GetSteps();
            msb = (SevenBitNumber)data;
            lsb = null;
        }

        protected override int GetIncrementStepsCount()
        {
            return GetSteps();
        }

        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps";
        }

        #endregion
    }
}

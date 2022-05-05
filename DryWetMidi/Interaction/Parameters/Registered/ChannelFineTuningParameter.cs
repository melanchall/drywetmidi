using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Channel Fine Tuning registered parameter.
    /// </summary>
    public sealed class ChannelFineTuningParameter : RegisteredParameter
    {
        #region Constants

        /// <summary>
        /// Represents the smallest possible number of cents to tune by.
        /// </summary>
        public const float MinCents = -100f;

        /// <summary>
        /// Represents the largest possible number of cents to tune by.
        /// </summary>
        public const float MaxCents = 100f;

        private const int CentsRangeSize = 16383;
        private const float CentResolution = CentsRangeSize / 200f;

        #endregion

        #region Fields

        private float _cents;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFineTuningParameter"/>.
        /// </summary>
        public ChannelFineTuningParameter()
            : base(RegisteredParameterType.ChannelFineTuning)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFineTuningParameter"/> with the specified
        /// exact number of cents.
        /// </summary>
        /// <param name="cents">The number of cents to tune by.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="cents"/> is out of
        /// [<see cref="MinCents"/>; <see cref="MaxCents"/>] range.</exception>
        public ChannelFineTuningParameter(float cents)
            : this(cents, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFineTuningParameter"/> with the specified
        /// number of cents and type of this number.
        /// </summary>
        /// <param name="cents">The number of cents to tune by.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="valueType"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="cents"/> is out of
        /// [<see cref="MinCents"/>; <see cref="MaxCents"/>] range.</exception>
        public ChannelFineTuningParameter(float cents, ParameterValueType valueType)
            : this()
        {
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of cents to tune by.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinCents"/>; <see cref="MaxCents"/>] range.</exception>
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
            return MathUtilities.EnsureInBounds((int)Math.Round((Cents + MaxCents) * CentResolution), 0, CentsRangeSize);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public override ITimedObject Clone()
        {
            return new ChannelFineTuningParameter(Cents, ValueType);
        }

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            var data = (ushort)GetSteps();
            msb = data.GetHead();
            lsb = data.GetTail();
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            return GetSteps();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: {Cents} cents";
        }

        #endregion
    }
}

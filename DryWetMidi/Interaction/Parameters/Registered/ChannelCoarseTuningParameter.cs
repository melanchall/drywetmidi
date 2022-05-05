using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Channel Coarse Tuning registered parameter.
    /// </summary>
    public sealed class ChannelCoarseTuningParameter : RegisteredParameter
    {
        #region Constants

        /// <summary>
        /// Represents the smallest possible number of half-steps to tune by.
        /// </summary>
        public const sbyte MinHalfSteps = -64;

        /// <summary>
        /// Represents the largest possible number of half-steps to tune by.
        /// </summary>
        public const sbyte MaxHalfSteps = 63;

        #endregion

        #region Fields

        private sbyte _halfSteps;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCoarseTuningParameter"/>.
        /// </summary>
        public ChannelCoarseTuningParameter()
            : base(RegisteredParameterType.ChannelCoarseTuning)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCoarseTuningParameter"/> with the specified
        /// exact number of half-steps.
        /// </summary>
        /// <param name="halfSteps">The number of half-steps to tune by.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of
        /// [<see cref="MinHalfSteps"/>; <see cref="MaxHalfSteps"/>] range.</exception>
        public ChannelCoarseTuningParameter(sbyte halfSteps)
            : this(halfSteps, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCoarseTuningParameter"/> with the specified
        /// number of half-steps and type of this number.
        /// </summary>
        /// <param name="halfSteps">Number of half-steps to tune by.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="valueType"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="halfSteps"/> is out of
        /// [<see cref="MinHalfSteps"/>; <see cref="MaxHalfSteps"/>] range.</exception>
        public ChannelCoarseTuningParameter(sbyte halfSteps, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of half-steps to tune by.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of
        /// [<see cref="MinHalfSteps"/>; <see cref="MaxHalfSteps"/>] range.</exception>
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

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public override ITimedObject Clone()
        {
            return new ChannelCoarseTuningParameter(HalfSteps, ValueType);
        }

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            var data = GetSteps();
            msb = (SevenBitNumber)data;
            lsb = null;
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            return GetSteps();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps";
        }

        #endregion
    }
}

using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Modulation Depth Range (Vibrato Depth Range) registered parameter.
    /// </summary>
    public sealed class ModulationDepthRangeParameter : RegisteredParameter
    {
        #region Constants

        /// <summary>
        /// Represents the default number of half-steps to tune by.
        /// </summary>
        public static readonly SevenBitNumber DefaultHalfSteps = (SevenBitNumber)0;

        /// <summary>
        /// Represents the default number of cents to tune by.
        /// </summary>
        public static readonly float DefaultCents = 50f;

        /// <summary>
        /// Represents the smallest possible number of cents to tune by.
        /// </summary>
        public const float MinCents = 0f;

        /// <summary>
        /// Represents the largest possible number of cents to tune by.
        /// </summary>
        public const float MaxCents = 100f;

        private const float CentResolution = 128 / 100f;

        #endregion

        #region Fields

        private float _cents = DefaultCents;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ModulationDepthRangeParameter"/>.
        /// </summary>
        public ModulationDepthRangeParameter()
            : base(RegisteredParameterType.ModulationDepthRange)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModulationDepthRangeParameter"/> with the specified
        /// exact number of half-steps and cents.
        /// </summary>
        /// <param name="halfSteps">The number of half-steps to tune by.</param>
        /// <param name="cents">The number of cents to tune by.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="cents"/> is out of
        /// [<see cref="MinCents"/>; <see cref="MaxCents"/>] range.</exception>
        public ModulationDepthRangeParameter(SevenBitNumber halfSteps, float cents)
            : this(halfSteps, cents, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModulationDepthRangeParameter"/> with the specified
        /// number of half-steps and cents and type of the data.
        /// </summary>
        /// <param name="halfSteps">The number of half-steps to tune by.</param>
        /// <param name="cents">the number of cents to tune by.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="valueType"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="cents"/> is out of
        /// [<see cref="MinCents"/>; <see cref="MaxCents"/>] range.</exception>
        public ModulationDepthRangeParameter(SevenBitNumber halfSteps, float cents, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of half-steps to tune by.
        /// </summary>
        public SevenBitNumber HalfSteps { get; set; } = DefaultHalfSteps;

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

        #region Overrides

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public override ITimedObject Clone()
        {
            return new ModulationDepthRangeParameter(HalfSteps, Cents, ValueType);
        }

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = HalfSteps;
            lsb = (SevenBitNumber)MathUtilities.EnsureInBounds((int)Math.Round(Cents * CentResolution), SevenBitNumber.MinValue, SevenBitNumber.MaxValue);
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            // TODO: find what is increment
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps, {Cents} cents";
        }

        #endregion
    }
}

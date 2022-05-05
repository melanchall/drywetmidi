using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Pitch Bend Sensitivity registered parameter.
    /// </summary>
    public sealed class PitchBendSensitivityParameter : RegisteredParameter
    {
        #region Constants

        /// <summary>
        /// Represents the default number of half-steps of the sensitivity range.
        /// </summary>
        public static readonly SevenBitNumber DefaultHalfSteps = (SevenBitNumber)2;

        /// <summary>
        /// Represents the default number of cents of the sensitivity range.
        /// </summary>
        public static readonly SevenBitNumber DefaultCents = (SevenBitNumber)0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendSensitivityParameter"/>.
        /// </summary>
        public PitchBendSensitivityParameter()
            : base(RegisteredParameterType.PitchBendSensitivity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendSensitivityParameter"/> with the specified
        /// exact number of half-steps and cents.
        /// </summary>
        /// <param name="halfSteps">The number of half-steps of the sensitivity range.</param>
        /// <param name="cents">The number of cents of the sensitivity range.</param>
        public PitchBendSensitivityParameter(SevenBitNumber halfSteps, SevenBitNumber cents)
            : this(halfSteps, cents, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModulationDepthRangeParameter"/> with the specified
        /// number of half-steps and cents and type of the data.
        /// </summary>
        /// <param name="halfSteps">The number of half-steps of the sensitivity range.</param>
        /// <param name="cents">the number of cents of the sensitivity range.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="valueType"/> specified an invalid value.</exception>
        public PitchBendSensitivityParameter(SevenBitNumber halfSteps, SevenBitNumber cents, ParameterValueType valueType)
            : this()
        {
            HalfSteps = halfSteps;
            Cents = cents;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the number of half-steps of the sensitivity range.
        /// </summary>
        public SevenBitNumber HalfSteps { get; set; } = DefaultHalfSteps;

        /// <summary>
        /// Gets or sets the number of cents of the sensitivity range.
        /// </summary>
        public SevenBitNumber Cents { get; set; } = DefaultCents;

        #endregion

        #region Overrides

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public override ITimedObject Clone()
        {
            return new PitchBendSensitivityParameter(HalfSteps, Cents, ValueType);
        }

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = HalfSteps;
            lsb = Cents;
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            return HalfSteps * 100 + Cents;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: {HalfSteps} half-steps, {Cents} cents";
        }

        #endregion
    }
}

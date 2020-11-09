using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Tuning Program Change registered parameter.
    /// </summary>
    public sealed class TuningProgramChangeParameter : RegisteredParameter
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningProgramChangeParameter"/>.
        /// </summary>
        public TuningProgramChangeParameter()
            : base(RegisteredParameterType.TuningProgramChange)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningProgramChangeParameter"/> with the specified
        /// exact program number.
        /// </summary>
        /// <param name="programNumber">The program number.</param>
        public TuningProgramChangeParameter(SevenBitNumber programNumber)
            : this(programNumber, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningProgramChangeParameter"/> with the specified
        /// program number and type of this number.
        /// </summary>
        /// <param name="programNumber">The program number.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        public TuningProgramChangeParameter(SevenBitNumber programNumber, ParameterValueType valueType)
            : this()
        {
            ProgramNumber = programNumber;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the program number.
        /// </summary>
        public SevenBitNumber ProgramNumber { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = ProgramNumber;
            lsb = null;
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            return ProgramNumber;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: program #{ProgramNumber}";
        }

        #endregion
    }
}

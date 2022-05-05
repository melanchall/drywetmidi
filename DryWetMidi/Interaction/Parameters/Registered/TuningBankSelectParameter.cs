using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Tuning Bank Select registered parameter.
    /// </summary>
    public sealed class TuningBankSelectParameter : RegisteredParameter
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningBankSelectParameter"/>.
        /// </summary>
        public TuningBankSelectParameter()
            : base(RegisteredParameterType.TuningBankSelect)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningBankSelectParameter"/> with the specified
        /// exact bank number.
        /// </summary>
        /// <param name="bankNumber">The bank number.</param>
        public TuningBankSelectParameter(SevenBitNumber bankNumber)
            : this(bankNumber, ParameterValueType.Exact)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TuningBankSelectParameter"/> with the specified
        /// bank number and type of this number.
        /// </summary>
        /// <param name="bankNumber">The bank number.</param>
        /// <param name="valueType">The type of parameter's data which defines whether it
        /// represents exact value, increment or decrement.</param>
        public TuningBankSelectParameter(SevenBitNumber bankNumber, ParameterValueType valueType)
            : this()
        {
            BankNumber = bankNumber;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the bank number.
        /// </summary>
        public SevenBitNumber BankNumber { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public override ITimedObject Clone()
        {
            return new TuningBankSelectParameter(BankNumber, ValueType);
        }

        /// <inheritdoc/>
        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = BankNumber;
            lsb = null;
        }

        /// <inheritdoc/>
        protected override int GetIncrementStepsCount()
        {
            return BankNumber;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}: bank #{BankNumber}";
        }

        #endregion
    }
}

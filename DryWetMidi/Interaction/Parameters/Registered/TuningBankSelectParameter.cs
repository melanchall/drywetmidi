using System;
using System.Collections.Generic;
using System.Text;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TuningBankSelectParameter : RegisteredParameter
    {
        #region Constructor

        public TuningBankSelectParameter()
            : base(RegisteredParameterType.TuningBankSelect)
        {
        }

        public TuningBankSelectParameter(SevenBitNumber bankNumber)
            : this(bankNumber, ParameterValueType.Exact)
        {
        }

        public TuningBankSelectParameter(SevenBitNumber bankNumber, ParameterValueType valueType)
            : this()
        {
            BankNumber = bankNumber;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        public SevenBitNumber BankNumber { get; set; }

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = BankNumber;
            lsb = null;
        }

        protected override int GetIncrementStepsCount()
        {
            return BankNumber;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: bank #{BankNumber}";
        }

        #endregion
    }
}

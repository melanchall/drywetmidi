using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class TuningProgramChangeParameter : RegisteredParameter
    {
        #region Constructor

        public TuningProgramChangeParameter()
            : base(RegisteredParameterType.TuningProgramChange)
        {
        }

        public TuningProgramChangeParameter(SevenBitNumber programNumber)
            : this(programNumber, ParameterValueType.Exact)
        {
        }

        public TuningProgramChangeParameter(SevenBitNumber programNumber, ParameterValueType valueType)
            : this()
        {
            ProgramNumber = programNumber;
            ValueType = valueType;
        }

        #endregion

        #region Properties

        public SevenBitNumber ProgramNumber { get; set; }

        #endregion

        #region Overrides

        protected override void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb)
        {
            msb = ProgramNumber;
            lsb = null;
        }

        protected override int GetIncrementStepsCount()
        {
            return ProgramNumber;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: program #{ProgramNumber}";
        }

        #endregion
    }
}

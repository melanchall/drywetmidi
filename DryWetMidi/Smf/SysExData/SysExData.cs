using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public abstract class SysExData
    {
        #region Methods

        protected static SevenBitNumber ToSevenBitNumber(byte value, SysExDataReadingSettings settings, string parameterName, bool leftJustified = false)
        {
            if (value > SevenBitNumber.MaxValue)
            {
                switch (settings.InvalidSysExDataParameterValuePolicy)
                {
                    case InvalidSysExDataParameterValuePolicy.Abort:
                        throw new InvalidSysExDataParameterException($"{value} is invalid value for {parameterName}.", value);
                    case InvalidSysExDataParameterValuePolicy.ReadValid:
                        if (leftJustified)
                            value >>= 1;
                        else
                            value &= SevenBitNumber.MaxValue;
                        break;
                    case InvalidSysExDataParameterValuePolicy.SnapToLimits:
                        value = SevenBitNumber.MaxValue;
                        break;
                }
            }

            return (SevenBitNumber)value;
        }

        protected static SevenBitNumber[] ToSevenBitNumbers(byte[] values, SysExDataReadingSettings settings, string parameterName, bool leftJustified = false)
        {
            var result = new SevenBitNumber[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                result[i] = ToSevenBitNumber(values[i], settings, $"byte #{i} of {parameterName}", leftJustified);
            }

            return result;
        }

        internal abstract void Read(MidiReader reader, SysExDataReadingSettings settings);

        #endregion
    }
}

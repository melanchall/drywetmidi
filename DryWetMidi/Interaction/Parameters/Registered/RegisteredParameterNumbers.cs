using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class RegisteredParameterNumbers
    {
        #region Constants

        private static readonly Dictionary<RegisteredParameterType, SevenBitNumber> Msbs =
            new Dictionary<RegisteredParameterType, SevenBitNumber>
            {
                [RegisteredParameterType.PitchBendSensitivity] = (SevenBitNumber)0x00,
                [RegisteredParameterType.ChannelFineTuning]    = (SevenBitNumber)0x00,
                [RegisteredParameterType.ChannelCoarseTuning]  = (SevenBitNumber)0x00,
                [RegisteredParameterType.TuningProgramChange]  = (SevenBitNumber)0x00,
                [RegisteredParameterType.TuningBankSelect]     = (SevenBitNumber)0x00,
                [RegisteredParameterType.ModulationDepthRange] = (SevenBitNumber)0x00
            };

        private static readonly Dictionary<RegisteredParameterType, SevenBitNumber> Lsbs =
            new Dictionary<RegisteredParameterType, SevenBitNumber>
            {
                [RegisteredParameterType.PitchBendSensitivity] = (SevenBitNumber)0x00,
                [RegisteredParameterType.ChannelFineTuning]    = (SevenBitNumber)0x01,
                [RegisteredParameterType.ChannelCoarseTuning]  = (SevenBitNumber)0x02,
                [RegisteredParameterType.TuningProgramChange]  = (SevenBitNumber)0x03,
                [RegisteredParameterType.TuningBankSelect]     = (SevenBitNumber)0x04,
                [RegisteredParameterType.ModulationDepthRange] = (SevenBitNumber)0x05
            };

        #endregion

        #region Methods

        public static SevenBitNumber GetMsb(RegisteredParameterType parameterType)
        {
            return Msbs[parameterType];
        }

        public static SevenBitNumber GetLsb(RegisteredParameterType parameterType)
        {
            return Lsbs[parameterType];
        }

        #endregion
    }
}

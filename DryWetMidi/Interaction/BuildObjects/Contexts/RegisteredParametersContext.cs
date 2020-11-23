using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RegisteredParametersContext : IBuildingContext
    {
        #region Properties

        public IDictionary<FourBitNumber, SevenBitNumber> RpnMsbs { get; } = new Dictionary<FourBitNumber, SevenBitNumber>();

        public IDictionary<FourBitNumber, SevenBitNumber> RpnLsbs { get; } = new Dictionary<FourBitNumber, SevenBitNumber>();

        #endregion
    }
}

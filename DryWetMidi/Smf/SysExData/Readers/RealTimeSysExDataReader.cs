using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class RealTimeSysExDataReader : UniversalSysExDataReader
    {
        #region Constants

        private static readonly Dictionary<UniversalSysExDataId, Type> SysExDataTypes = new Dictionary<UniversalSysExDataId, Type>
        {
        };

        #endregion

        #region Constructor

        public RealTimeSysExDataReader()
            : base(SysExDataTypes)
        {
        }

        #endregion
    }
}

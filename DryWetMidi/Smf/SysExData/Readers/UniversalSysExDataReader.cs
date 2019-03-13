using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    internal abstract class UniversalSysExDataReader : ISysExDataReader
    {
        #region Fields

        private readonly Dictionary<UniversalSysExDataId, Type> _sysExDataTypes;

        #endregion

        #region Constructor

        public UniversalSysExDataReader(Dictionary<UniversalSysExDataId, Type> sysExDataTypes)
        {
            _sysExDataTypes = sysExDataTypes;
        }

        #endregion

        #region ISysExDataReader

        public SysExData Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            var statusByte = reader.ReadByte();
            var deviceId = reader.ReadByte();
            var subId1 = reader.ReadByte();

            Type sysExDataType;
            if (!_sysExDataTypes.TryGetValue(new UniversalSysExDataId(subId1), out sysExDataType) &&
                !_sysExDataTypes.TryGetValue(new UniversalSysExDataId(subId1, reader.ReadByte()), out sysExDataType))
                throw new Exception("FIX ME"); // TODO: proper exception

            var sysExData = (UniversalSysExData)Activator.CreateInstance(sysExDataType);

            // TODO: throw on out of 7-bit number
            sysExData.DeviceId = (SevenBitNumber)deviceId;
            sysExData.Read(reader, settings);

            return sysExData;
        }

        #endregion
    }
}

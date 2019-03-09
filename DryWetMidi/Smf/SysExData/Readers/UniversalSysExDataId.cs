namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class UniversalSysExDataId
    {
        #region Constructor

        public UniversalSysExDataId(byte subId)
            : this(subId, null)
        {
        }

        public UniversalSysExDataId(byte subId1, byte? subId2)
        {
            SubId1 = subId1;
            SubId2 = subId2;
        }

        #endregion

        #region Properties

        public byte SubId1 { get; }

        public byte? SubId2 { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            var universalSysExDataId = obj as UniversalSysExDataId;
            if (universalSysExDataId == null)
                return false;

            return universalSysExDataId.SubId1 == SubId1 &&
                   universalSysExDataId.SubId2 == SubId2;
        }

        public override int GetHashCode()
        {
            return SubId1.GetHashCode() ^ (SubId2?.GetHashCode() ?? 0);
        }

        #endregion
    }
}

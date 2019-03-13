namespace Melanchall.DryWetMidi.Smf
{
    public enum MtcSpecialType : ushort
    {
        TimeCodeOffset   = 0,
        EnableEventList  = 1,
        DisableEventList = 2,
        ClearEventList   = 3,
        SystemStop       = 4,
        EventListRequest = 5
    }
}

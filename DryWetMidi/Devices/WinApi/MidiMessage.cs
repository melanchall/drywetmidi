namespace Melanchall.DryWetMidi.Devices
{
    // TODO: filter out
    internal enum MidiMessage : int
    {
        MM_MIM_CLOSE = 962,
        MM_MIM_DATA = 963,
        MM_MIM_ERROR = 965,
        MM_MIM_LONGDATA = 964,
        MM_MIM_LONGERROR = 966,
        MM_MIM_MOREDATA = 972,
        MM_MIM_OPEN = 961,
        MIM_CLOSE = MM_MIM_CLOSE,
        MIM_DATA = MM_MIM_DATA,
        MIM_ERROR = MM_MIM_ERROR,
        MIM_LONGDATA = MM_MIM_LONGDATA,
        MIM_LONGERROR = MM_MIM_LONGERROR,
        MIM_MOREDATA = MM_MIM_MOREDATA,
        MIM_OPEN = MM_MIM_OPEN,
        MM_MOM_CLOSE = 968,
        MM_MOM_DONE = 969,
        MM_MOM_OPEN = 967,
        MM_MOM_POSITIONCB = 970
    }
}

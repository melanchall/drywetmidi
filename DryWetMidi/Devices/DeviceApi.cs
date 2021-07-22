namespace Melanchall.DryWetMidi.Devices
{
    internal abstract class DeviceApi
    {
        #region Enums

        public enum MidiMessage
        {
            MIM_CLOSE = 962,
            MIM_DATA = 963,
            MIM_ERROR = 965,
            MIM_LONGDATA = 964,
            MIM_LONGERROR = 966,
            MIM_MOREDATA = 972,
            MIM_OPEN = 961,
            MOM_CLOSE = 968,
            MOM_DONE = 969,
            MOM_OPEN = 967,
            MOM_POSITIONCB = 970
        }

        #endregion
    }
}

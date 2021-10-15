namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class CommonApi : NativeApi
    {
        #region Nested enums

        public enum API_TYPE
        {
            API_TYPE_WIN = 0,
            API_TYPE_MAC = 1
        }

        #endregion

        #region Methods

        public abstract API_TYPE Api_GetApiType();

        public abstract bool Api_CanCompareDevices();

        #endregion
    }
}

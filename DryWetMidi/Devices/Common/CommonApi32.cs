using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class CommonApi32 : CommonApi
    {
        #region Constants

        private const string LibraryName = LibraryName32;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true)]
        private static extern API_TYPE GetApiType();

        #endregion

        #region Methods

        public override API_TYPE Api_GetApiType()
        {
            return GetApiType();
        }

        #endregion
    }
}

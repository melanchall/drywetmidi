using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static TickGeneratorApi _api;

        #endregion

        #region Properties

        public static TickGeneratorApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (TickGeneratorApi)new TickGeneratorApi64() : new TickGeneratorApi32();

                return _api;
            }
        }

        #endregion
    }
}

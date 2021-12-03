using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorSessionApiProvider
    {
        #region Constants

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        #endregion

        #region Fields

        private static TickGeneratorSessionApi _api;

        #endregion

        #region Properties

        public static TickGeneratorSessionApi Api
        {
            get
            {
                if (_api == null)
                    _api = Is64Bit ? (TickGeneratorSessionApi)new TickGeneratorSessionApi64() : new TickGeneratorSessionApi32();

                return _api;
            }
        }

        #endregion
    }
}

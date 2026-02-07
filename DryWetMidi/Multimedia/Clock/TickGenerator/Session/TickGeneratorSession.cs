using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorSession
    {
        #region Fields

        private static readonly object _lockObject = new object();

        private static IntPtr _handle;

        #endregion

        #region Methods

        public static IntPtr GetSessionHandle()
        {
            if (_handle == IntPtr.Zero)
            {
                lock (_lockObject)
                {
                    if (_handle == IntPtr.Zero)
                    {
                        var result = TickGeneratorSessionApi.Api_OpenSession(out _handle, out var errorCode);
                        NativeApiUtilities.HandleTickGeneratorNativeApiResult(result, errorCode);
                    }
                }
            }

            return _handle;
        }

        #endregion
    }
}

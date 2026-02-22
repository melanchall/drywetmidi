using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorSession
    {
        #region Fields

        private static readonly object _lockObject = new object();

        private static TickGeneratorSessionHandle _handle;

        #endregion

        #region Properties

#if TEST
        internal static TestCheckpoints TestCheckpoints { get; set; }
#endif

        #endregion

        #region Methods

        public static TickGeneratorSessionHandle GetSessionHandle()
        {
            if (_handle == null || _handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle == null || _handle.IsInvalid)
                    {
                        var rawHandle = IntPtr.Zero;

                        var result = TickGeneratorSessionApi.Api_OpenSession(out rawHandle, out var errorCode);
                        NativeApiUtilities.HandleTickGeneratorNativeApiResult(result, errorCode);

                        _handle = new TickGeneratorSessionHandle(rawHandle);

#if TEST
                        _handle.TestCheckpoints = TestCheckpoints;
#endif

                        AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                        AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                    }
                }
            }

            return _handle;
        }

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_handle != null && !_handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle != null && !_handle.IsInvalid)
                    {
                        _handle?.Dispose();
                        _handle = null;
                    }
                }
            }
        }

        #endregion
    }
}

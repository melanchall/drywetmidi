using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class TickGeneratorSessionApi32 : TickGeneratorSessionApi
    {
        #region Constants

        private const string LibraryName = LibraryName32;

        #endregion

        #region Extern functions

        [DllImport(LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern TGSESSION_OPENRESULT OpenTickGeneratorSession(out IntPtr handle);

        #endregion

        #region Methods

        public override TGSESSION_OPENRESULT Api_OpenSession(out IntPtr handle)
        {
            return OpenTickGeneratorSession(out handle);
        }

        #endregion
    }
}

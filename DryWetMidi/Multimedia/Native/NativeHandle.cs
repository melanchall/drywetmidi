using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class NativeHandle : SafeHandle
    {
        #region Constructor

        protected NativeHandle(IntPtr validHandle)
                : base(IntPtr.Zero, true)
        {
            SetHandle(validHandle);
        }

        #endregion

        #region Properties

        public IntPtr DeviceHandle
        {
            get { return handle; }
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        #endregion
    }
}

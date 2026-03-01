using Melanchall.DryWetMidi.Common;
using Microsoft.Win32.SafeHandles;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class NativeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public NativeHandle()
            : base(true)
        {
        }

        public NativeHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        public object Lock { get; } = new object();

#if TEST
        public TestCheckpoints TestCheckpoints { get; set; }
#endif
    }
}

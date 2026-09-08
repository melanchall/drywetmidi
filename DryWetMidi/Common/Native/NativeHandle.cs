using Melanchall.DryWetMidi.Common;
using Microsoft.Win32.SafeHandles;
using System;

namespace Melanchall.DryWetMidi.Common
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

#if NET9_0_OR_GREATER
        public System.Threading.Lock Lock { get; } = new System.Threading.Lock();
#else
        public object Lock { get; } = new object();
#endif

#if TEST
        public TestCheckpoints? TestCheckpoints { get; set; }
#endif
    }
}

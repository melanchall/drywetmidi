using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal abstract class EndpointHandle : NativeHandle
    {
        public EndpointHandle()
            : base()
        {
        }

        public EndpointHandle(IntPtr infoHandle)
            : base(infoHandle)
        {
        }

        public IntPtr OpenedEndpointHandle { get; set; } = IntPtr.Zero;
    }
}

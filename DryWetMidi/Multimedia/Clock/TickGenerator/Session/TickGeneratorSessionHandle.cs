using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class TickGeneratorSessionHandle : NativeHandle
    {
        public TickGeneratorSessionHandle()
            : base()
        {
        }

        public TickGeneratorSessionHandle(IntPtr handle)
            : base(handle)
        {
        }

        protected override bool ReleaseHandle()
        {
#if TEST
            TestCheckpoints?.SetCheckpointReached(TickGeneratorSessionCheckpointNames.HandleFinalizerEntered);
#endif

            var result = TickGeneratorSessionApi.Api_CloseSession(handle, out var errorCode);

#if TEST
            if (result != TickGeneratorSessionApi.TGSESSION_CLOSERESULT.TGSESSION_CLOSERESULT_OK)
                return false;

            TestCheckpoints?.SetCheckpointReached(TickGeneratorSessionCheckpointNames.SessionClosedInHandleFinalizer);
#endif

            return true;
        }
    }
}

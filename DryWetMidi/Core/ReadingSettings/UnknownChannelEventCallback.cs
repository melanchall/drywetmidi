using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public delegate UnknownChannelEventAction UnknownChannelEventCallback(FourBitNumber statusByte, FourBitNumber channel);
}

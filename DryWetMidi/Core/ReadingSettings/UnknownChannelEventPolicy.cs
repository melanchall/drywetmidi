namespace Melanchall.DryWetMidi.Core
{
    public enum UnknownChannelEventPolicy
    {
        Abort = 0,
        SkipStatusByte,
        SkipStatusByteAndOneDataByte,
        SkipStatusByteAndTwoDataBytes,
        UseCallback
    }
}

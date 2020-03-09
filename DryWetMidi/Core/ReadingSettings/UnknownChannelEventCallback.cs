using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Callback used to read unknown channel event if <see cref="ReadingSettings.UnknownChannelEventPolicy"/>
    /// set to <see cref="UnknownChannelEventPolicy.UseCallback"/>.
    /// </summary>
    /// <param name="statusByte">Status byte of channel event.</param>
    /// <param name="channel">Channel of event.</param>
    /// <returns>An instance of the <see cref="UnknownChannelEventAction"/> representing an action to perform.</returns>
    public delegate UnknownChannelEventAction UnknownChannelEventCallback(FourBitNumber statusByte, FourBitNumber channel);
}

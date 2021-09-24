namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on 0xFF status byte since it can mean
    /// either <see cref="ResetEvent"/> or <see cref="MetaEvent"/>.
    /// The default is <see cref="ReadAsResetEvent"/>.
    /// </summary>
    public enum FfStatusBytePolicy
    {
        /// <summary>
        /// Read event as an instance <see cref="ResetEvent"/>.
        /// </summary>
        ReadAsResetEvent = 0,

        /// <summary>
        /// Read event as an instance dirived from <see cref="MetaEvent"/>.
        /// </summary>
        ReadAsMetaEvent
    }
}

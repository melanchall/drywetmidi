namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on invalid value of a channel event's
    /// parameter value. Valid values are 0-127 so, for example, 128 is the invalid one
    /// and will be processed according to this policy. The default is <see cref="Abort"/>.
    /// </summary>
    public enum InvalidChannelEventParameterValuePolicy : byte
    {
        /// <summary>
        /// Abort reading and throw an <see cref="InvalidChannelEventParameterValueException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Read byte and take its lower (rightmost) seven bits as the final value.
        /// </summary>
        ReadValid,

        /// <summary>
        /// Read value and snap it to limits of the allowable range if it is out of them.
        /// </summary>
        SnapToLimits
    }
}

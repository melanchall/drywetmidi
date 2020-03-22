namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should react on invalid value of a meta event's
    /// parameter value. For example, <c>255</c> is the invalid value for the <see cref="KeySignatureEvent.Scale"/>
    /// and will be processed according with this policy. The default is <see cref="Abort"/>.
    /// </summary>
    public enum InvalidMetaEventParameterValuePolicy
    {
        /// <summary>
        /// Abort reading and throw an <see cref="InvalidMetaEventParameterValueException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Read value and snap it to limits of the allowable range if it is out of them.
        /// </summary>
        SnapToLimits
    }
}

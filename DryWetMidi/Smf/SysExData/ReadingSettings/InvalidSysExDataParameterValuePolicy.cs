namespace Melanchall.DryWetMidi.Smf
{
    public enum InvalidSysExDataParameterValuePolicy
    {
        /// <summary>
        /// Abort reading and throw an <see cref="InvalidSysExDataParameterException"/>.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Read byte and take its lower seven bits as the final value.
        /// </summary>
        ReadValid,

        /// <summary>
        /// Read value and snap it to limits of the allowable range if it is out of them.
        /// </summary>
        SnapToLimits
    }
}

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Specifies how reading engine should read zero-length objects such as strings or arrays.
    /// The default is <see cref="ReadAsEmptyObject"/>.
    /// </summary>
    public enum ZeroLengthDataPolicy
    {
        /// <summary>
        /// Read as an object with length of zero.
        /// </summary>
        ReadAsEmptyObject = 0,

        /// <summary>
        /// Read as <c>null</c>.
        /// </summary>
        ReadAsNull
    }
}

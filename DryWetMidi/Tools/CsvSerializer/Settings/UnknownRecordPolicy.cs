namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Specifies how CSV deserialization engine should react on an unknown record.
    /// The default value is <see cref="Abort"/>.
    /// </summary>
    public enum UnknownRecordPolicy
    {
        /// <summary>
        /// Abort the deserialization process and throw an exception.
        /// </summary>
        Abort = 0,

        /// <summary>
        /// Ignore the record and continue deserialization process.
        /// </summary>
        Ignore,
    }
}

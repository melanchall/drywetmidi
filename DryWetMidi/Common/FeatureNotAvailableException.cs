namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// The exception that is thrown when a feature is not available on the current system.
    /// </summary>
    public sealed class FeatureNotAvailableException : MidiException
    {
        #region Constructor

        internal FeatureNotAvailableException(string message)
            : base(message)
        {
        }

        #endregion
    }
}

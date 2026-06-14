namespace Melanchall.DryWetMidi.Common
{
    // TODO: triple-slash comments in usages
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

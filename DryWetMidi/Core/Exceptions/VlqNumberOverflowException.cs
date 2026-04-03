using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class VlqNumberOverflowException : MidiException
    {
        #region Constructors

        public VlqNumberOverflowException(string message)
            : base(message)
        {
        }

        #endregion
    }
}

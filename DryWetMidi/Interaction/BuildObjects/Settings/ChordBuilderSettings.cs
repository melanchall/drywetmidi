using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChordBuilderSettings
    {
        #region Fields

        private long _notesTolerance = 0;

        #endregion

        #region Properties

        public long NotesTolerance
        {
            get { return _notesTolerance; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Value is negative.");

                _notesTolerance = value;
            }
        }

        #endregion
    }
}

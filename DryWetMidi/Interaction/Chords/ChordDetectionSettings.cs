using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ChordDetectionSettings
    {
        #region Constants

        private const int DefaultNotesMinCount = 1;
        private static readonly long DefaultNotesTolerance = 0;

        #endregion

        #region Fields

        private int _notesMinCount = DefaultNotesMinCount;
        private long _notesTolerance = DefaultNotesTolerance;

        #endregion

        #region Properties

        public int NotesMinCount
        {
            get { return _notesMinCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _notesMinCount = value;
            }
        }

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

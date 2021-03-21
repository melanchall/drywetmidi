using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class NoteDetectionSettings
    {
        #region Fields

        private NoteStartDetectionPolicy _noteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn;

        #endregion

        #region Properties

        public NoteStartDetectionPolicy NoteStartDetectionPolicy
        {
            get { return _noteStartDetectionPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteStartDetectionPolicy = value;
            }
        }

        #endregion
    }
}

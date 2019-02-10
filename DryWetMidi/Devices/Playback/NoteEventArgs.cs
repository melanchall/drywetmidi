using System;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class NoteEventArgs : EventArgs
    {
        #region Constructor

        internal NoteEventArgs(Note note)
        {
            Note = note;
        }

        #endregion

        #region Properties

        public Note Note { get; }

        #endregion
    }
}

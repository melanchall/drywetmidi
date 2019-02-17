using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class NotesEventArgs : EventArgs
    {
        #region Constructor

        internal NotesEventArgs(params Note[] notes)
        {
            Notes = notes;
        }

        #endregion

        #region Properties

        public IEnumerable<Note> Notes { get; }

        #endregion
    }
}

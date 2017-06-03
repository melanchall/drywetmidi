using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class NotesCollectionChangedEventArgs
    {
        #region Constructor

        public NotesCollectionChangedEventArgs(IEnumerable<Note> addedNotes, IEnumerable<Note> removedNotes)
        {
            AddedNotes = addedNotes;
            RemovedNotes = removedNotes;
        }

        #endregion

        #region Properties

        public IEnumerable<Note> AddedNotes { get; }

        public IEnumerable<Note> RemovedNotes { get; }

        #endregion
    }

    public delegate void NotesCollectionChangedEventHandler(NotesCollection collection, NotesCollectionChangedEventArgs args);
}

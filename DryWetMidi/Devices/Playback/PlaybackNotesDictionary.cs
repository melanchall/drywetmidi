using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class PlaybackNotesDictionary : IEnumerable<PlaybackNotesDictionaryEntry>
    {
        #region Fields

        private readonly Dictionary<NoteId, Note> _notes = new Dictionary<NoteId, Note>();

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void AddOrUpdate(NoteId noteId, Note note)
        {
            _notes[noteId] = note;
        }

        public void Remove(NoteId noteId)
        {
            _notes.Remove(noteId);
        }

        public void Clear()
        {
            _notes.Clear();
        }

        #endregion

        #region IEnumerable<PlaybackNotesDictionaryEntry>

        public IEnumerator<PlaybackNotesDictionaryEntry> GetEnumerator()
        {
            return _notes.Select(n => new PlaybackNotesDictionaryEntry(n.Key, n.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

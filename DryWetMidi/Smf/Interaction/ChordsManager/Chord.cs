using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Chord
    {
        #region Events

        public event NotesCollectionChangedEventHandler NotesCollectionChanged;

        #endregion

        #region Constructor

        public Chord()
            : this(Enumerable.Empty<Note>())
        {
        }

        public Chord(IEnumerable<Note> notes)
            : this(notes, 0)
        {
        }

        public Chord(IEnumerable<Note> notes, long time)
        {
            Notes = new NotesCollection(notes);
            Notes.CollectionChanged += OnNotesCollectionChanged;

            Time = time;
        }

        #endregion

        #region Properties

        public NotesCollection Notes { get; }

        public long Time
        {
            get { return Notes.FirstOrDefault()?.Time ?? 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Time is negative.");

                var currentTime = Time;

                foreach (var note in Notes)
                {
                    var offset = note.Time - currentTime;
                    note.Time = value + offset;
                }
            }
        }

        public long Length
        {
            get
            {
                if (!Notes.Any())
                    return 0;

                var startTime = long.MaxValue;
                var endTime = long.MinValue;

                foreach (var note in Notes)
                {
                    var noteStartTime = note.Time;
                    if (noteStartTime < startTime)
                        startTime = noteStartTime;

                    var noteEndTime = noteStartTime + note.Length;
                    if (noteEndTime > endTime)
                        endTime = noteEndTime;
                }

                return endTime - startTime;
            }
        }

        #endregion

        #region Methods

        private void OnNotesCollectionChanged(NotesCollection collection, NotesCollectionChangedEventArgs args)
        {
            NotesCollectionChanged?.Invoke(collection, args);
        }

        #endregion
    }
}

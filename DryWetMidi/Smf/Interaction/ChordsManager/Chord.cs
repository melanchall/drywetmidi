using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Chord
    {
        #region Fields

        private readonly List<Note> _notes = new List<Note>();

        #endregion

        #region Constructor

        public Chord()
        {
        }

        public Chord(IEnumerable<Note> notes)
            : this(notes, 0)
        {
        }

        public Chord(IEnumerable<Note> notes, long time)
        {
            _notes.AddRange(notes);
            Time = time;
        }

        #endregion

        #region Properties

        public IEnumerable<Note> Notes => _notes.OrderBy(n => n.Time);

        public long Time
        {
            get { return Notes.FirstOrDefault()?.Time ?? 0; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Time is negative.");

                // TODO
            }
        }

        #endregion

        #region Methods

        public void AddNote(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            _notes.Add(note);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a musical chord.
    /// </summary>
    public sealed class Chord : ILengthedObject
    {
        #region Events

        /// <summary>
        /// Occurs when notes collection changes.
        /// </summary>
        public event NotesCollectionChangedEventHandler NotesCollectionChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/>.
        /// </summary>
        public Chord()
            : this(Enumerable.Empty<Note>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified
        /// collection of notes.
        /// </summary>
        /// <param name="notes">Notes to combine into an chord.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public Chord(IEnumerable<Note> notes)
            : this(notes, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified
        /// collection of notes and chord time.
        /// </summary>
        /// <param name="notes">Notes to combine into an chord.</param>
        /// <param name="time">Time of the chord which is time of the earliest note of the <paramref name="notes"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public Chord(IEnumerable<Note> notes, long time)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            Notes = new NotesCollection(notes);
            Notes.CollectionChanged += OnNotesCollectionChanged;

            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <see cref="NotesCollection"/> that represents notes of this chord.
        /// </summary>
        public NotesCollection Notes { get; }

        /// <summary>
        /// Gets or sets absolute time of the chord in units defined by the time division of a MIDI file.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Time is negative.</exception>
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

        /// <summary>
        /// Gets length of the chord in units defined by the time division of a MIDI file.
        /// </summary>
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

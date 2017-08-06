using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        /// <param name="notes">Notes to combine into a chord.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public Chord(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            Notes = new NotesCollection(notes);
            Notes.CollectionChanged += OnNotesCollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified
        /// collection of notes.
        /// </summary>
        /// <param name="notes">Notes to combine into a chord.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        public Chord(params Note[] notes)
            : this(notes as IEnumerable<Note>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified
        /// collection of notes and chord time.
        /// </summary>
        /// <param name="notes">Notes to combine into a chord.</param>
        /// <param name="time">Time of the chord which is time of the earliest note of the <paramref name="notes"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public Chord(IEnumerable<Note> notes, long time)
            : this(notes)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

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
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

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

        public FourBitNumber Channel
        {
            get => GetNotesProperty(n => n.Channel, "Chord's notes have different channels.");
            set => SetNotesProperty(n => n.Channel, value);
        }

        public SevenBitNumber Velocity
        {
            get => GetNotesProperty(n => n.Velocity, "Chord's notes have different velocities.");
            set => SetNotesProperty(n => n.Velocity, value);
        }

        public SevenBitNumber OffVelocity
        {
            get => GetNotesProperty(n => n.OffVelocity, "Chord's notes have different off velocities.");
            set => SetNotesProperty(n => n.OffVelocity, value);
        }

        #endregion

        #region Methods

        private void OnNotesCollectionChanged(NotesCollection collection, NotesCollectionChangedEventArgs args)
        {
            NotesCollectionChanged?.Invoke(collection, args);
        }

        private TValue GetNotesProperty<TValue>(Func<Note, TValue> valueSelector, string differentValuesMessage)
        {
            if (!Notes.Any())
                throw new InvalidOperationException("Chord doesn't contain notes.");

            var values = Notes.Select(valueSelector).Distinct().ToArray();
            if (values.Length > 1)
                throw new InvalidOperationException(differentValuesMessage);

            return values.First();
        }

        private void SetNotesProperty<TValue>(Expression<Func<Note, TValue>> propertySelector, TValue value)
        {
            var propertySelectorExpression = propertySelector.Body as MemberExpression;
            if (propertySelectorExpression == null)
                return;

            var property = propertySelectorExpression.Member as PropertyInfo;
            if (property == null)
                return;

            foreach (var note in Notes)
            {
                property.SetValue(note, value);
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var notes = Notes;
            return notes.Any()
                ? string.Join(" ", notes.OrderBy(n => n.NoteNumber))
                : "Empty notes collection";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var chord = obj as Chord;
            if (chord == null)
                return false;

            return Notes.SequenceEqual(chord.Notes);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int result = 0;

            foreach (var note in Notes)
            {
                result = ((result << 5) + result) ^ note.GetHashCode();
            }

            return result;
        }

        #endregion
    }
}

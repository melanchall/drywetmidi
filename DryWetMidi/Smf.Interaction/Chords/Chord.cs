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
    public sealed class Chord : ILengthedObject, IMusicalObject
    {
        #region Events

        /// <summary>
        /// Occurs when notes collection changes.
        /// </summary>
        public event NotesCollectionChangedEventHandler NotesCollectionChanged;

        #endregion

        #region Constants

        private static readonly Expression<Func<Note, FourBitNumber>> ChannelPropertySelector = n => n.Channel;
        private static readonly Expression<Func<Note, SevenBitNumber>> VelocityPropertySelector = n => n.Velocity;
        private static readonly Expression<Func<Note, SevenBitNumber>> OffVelocityPropertySelector = n => n.OffVelocity;

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
        /// Gets absolute time of the chord in units defined by the time division of a MIDI file.
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
                    startTime = Math.Min(noteStartTime, startTime);

                    var noteEndTime = noteStartTime + note.Length;
                    endTime = Math.Max(noteEndTime, endTime);
                }

                return endTime - startTime;
            }
            set
            {
                var lengthChange = value - Length;

                foreach (var note in Notes)
                {
                    note.Length += lengthChange;
                }
            }
        }

        /// <summary>
        /// Gets or sets channel to play the chord on.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get channel since a chord doesn't contain notes.
        /// -or- Unable to get channel since chord's notes have different <see cref="Note.Velocity"/>.</exception>
        public FourBitNumber Channel
        {
            get { return GetNotesProperty(ChannelPropertySelector); }
            set { SetNotesProperty(ChannelPropertySelector, value); }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOnEvent"/> events of a chord's notes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get velocity since a chord doesn't contain notes.
        /// -or- Unable to get velocity since chord's notes have different <see cref="Note.Velocity"/>.</exception>
        public SevenBitNumber Velocity
        {
            get { return GetNotesProperty(VelocityPropertySelector); }
            set { SetNotesProperty(VelocityPropertySelector, value); }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOffEvent"/> events of a chord's notes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get off velocity since a chord doesn't contain notes.
        /// -or- Unable to get off velocity since chord's notes have different <see cref="Note.OffVelocity"/>.</exception>
        public SevenBitNumber OffVelocity
        {
            get { return GetNotesProperty(OffVelocityPropertySelector); }
            set { SetNotesProperty(OffVelocityPropertySelector, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clones chord by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the chord.</returns>
        public Chord Clone()
        {
            return new Chord(Notes.Select(note => note.Clone()));
        }

        /// <summary>
        /// Splits the current <see cref="Chord"/> by the specified time.
        /// </summary>
        /// <remarks>
        /// If <paramref name="time"/> is less than time of the chord, the left part will be null.
        /// If <paramref name="time"/> is greater than end time of the chord, the right part
        /// will be null.
        /// </remarks>
        /// <param name="time">Time to split the chord by.</param>
        /// <returns>An object containing left and right parts of the splitted <see cref="Chord"/>.
        /// Both parts are instances of <see cref="Chord"/> too.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public SplittedLengthedObject<Chord> Split(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = Time;
            var endTime = startTime + Length;

            if (time <= startTime)
                return new SplittedLengthedObject<Chord>(null, Clone());

            if (time >= endTime)
                return new SplittedLengthedObject<Chord>(Clone(), null);

            //

            var parts = Notes.Select(n => n.Split(time)).ToArray();

            var leftPart = new Chord(parts.Select(p => p.LeftPart).Where(p => p != null));
            var rightPart = new Chord(parts.Select(p => p.RightPart).Where(p => p != null));

            return new SplittedLengthedObject<Chord>(leftPart, rightPart);
        }

        private void OnNotesCollectionChanged(NotesCollection collection, NotesCollectionChangedEventArgs args)
        {
            NotesCollectionChanged?.Invoke(collection, args);
        }

        /// <summary>
        /// Gets value of the specified note's property.
        /// </summary>
        /// <typeparam name="TValue">Type of a note's property.</typeparam>
        /// <param name="propertySelector">Expression that represent a note's property.</param>
        /// <returns>Value of the specified note's property.</returns>
        /// <exception cref="InvalidOperationException">Chord doesn't contain notes. -or-
        /// Chord's notes have different values of the specified property.</exception>
        private TValue GetNotesProperty<TValue>(Expression<Func<Note, TValue>> propertySelector)
        {
            if (!Notes.Any())
                throw new InvalidOperationException("Chord doesn't contain notes.");

            var propertyInfo = GetPropertyInfo(propertySelector);

            var values = Notes.Select(n => (TValue)propertyInfo.GetValue(n)).Distinct().ToArray();
            if (values.Length > 1)
                throw new InvalidOperationException($"Chord's notes have different values of the {propertyInfo.Name} property.");

            return values.First();
        }

        private void SetNotesProperty<TValue>(Expression<Func<Note, TValue>> propertySelector, TValue value)
        {
            var propertyInfo = GetPropertyInfo(propertySelector);

            foreach (var note in Notes)
            {
                propertyInfo.SetValue(note, value);
            }
        }

        private static PropertyInfo GetPropertyInfo<TValue>(Expression<Func<Note, TValue>> propertySelector)
        {
            return (propertySelector.Body as MemberExpression)?.Member as PropertyInfo;
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

        #endregion
    }
}

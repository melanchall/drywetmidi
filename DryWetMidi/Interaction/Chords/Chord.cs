using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a musical chord.
    /// </summary>
    public class Chord : ILengthedObject, IMusicalObject, INotifyTimeChanged, INotifyLengthChanged
    {
        #region Events

        /// <summary>
        /// Occurs when notes collection changes.
        /// </summary>
        public event TimedObjectsCollectionChangedEventHandler<Note> NotesCollectionChanged;

        /// <summary>
        /// Occurs when the time of an object has been changed.
        /// </summary>
        public event EventHandler<TimeChangedEventArgs> TimeChanged;

        /// <summary>
        /// Occurs when the length of an object has been changed.
        /// </summary>
        public event EventHandler<LengthChangedEventArgs> LengthChanged;

        #endregion

        #region Fields

        private FourBitNumber? _channel;
        private SevenBitNumber? _velocity;
        private SevenBitNumber? _offVelocity;

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
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        public Chord(IEnumerable<Note> notes)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            Notes = new TimedObjectsCollection<Note>(notes, null);
            Notes.CollectionChanged += OnNotesCollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified
        /// collection of notes.
        /// </summary>
        /// <param name="notes">Notes to combine into a chord.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
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
        /// Gets a <see cref="TimedObjectsCollection{TObject}"/> that contains notes of this chord.
        /// </summary>
        public TimedObjectsCollection<Note> Notes { get; }

        /// <summary>
        /// Gets or sets absolute time of the chord in units defined by the time division of a MIDI file.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the time in different representations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public long Time
        {
            get { return Notes.FirstOrDefault()?.Time ?? 0; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                var oldTime = Time;
                if (value == oldTime)
                    return;

                foreach (var note in Notes)
                {
                    var offset = note.Time - oldTime;
                    note.Time = value + offset;
                }

                TimeChanged?.Invoke(this, new TimeChangedEventArgs(oldTime, value));
            }
        }

        /// <summary>
        /// Gets or sets the length of the chord in units defined by the time division of a MIDI file.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the length in different representations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
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
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                var oldLength = Length;
                if (value == oldLength)
                    return;

                var lastNoteTime = 0L;
                var firstNoteTime = long.MaxValue;

                foreach (var note in Notes)
                {
                    if (note.Time > lastNoteTime)
                        lastNoteTime = note.Time;

                    if (note.Time < firstNoteTime)
                        firstNoteTime = note.Time;
                }

                var lastNoteShift = lastNoteTime - firstNoteTime;
                ThrowIfArgument.IsLessThan(nameof(value), value, lastNoteShift, $"Value is less than {lastNoteShift}. Length must not be less than the distance between chord's start and its last note.");

                var lengthChange = value - Length;

                foreach (var note in Notes)
                {
                    note.Length += lengthChange;
                }

                LengthChanged?.Invoke(this, new LengthChangedEventArgs(oldLength, value));
            }
        }

        /// <summary>
        /// Gets the end time of an object.
        /// </summary>
        public long EndTime => Time + Length;

        /// <summary>
        /// Gets or sets channel to play the chord on.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Unable to get channel since a chord doesn't contain notes.</description>
        /// </item>
        /// <item>
        /// <description>Unable to get channel since chord's notes have different <see cref="Note.Velocity"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public FourBitNumber Channel
        {
            get
            {
                if (_channel != null)
                    return _channel.Value;

                FourBitNumber? channel = null;

                foreach (var note in Notes)
                {
                    if (channel != null && note.Channel != channel)
                        throw new InvalidOperationException("Chord's notes have different channels.");

                    channel = note.Channel;
                }

                if (channel == null)
                    throw new InvalidOperationException("Chord is empty.");

                return (_channel = channel).Value;
            }
            set
            {
                foreach (var note in Notes)
                {
                    note.Channel = value;
                }

                _channel = value;
            }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOnEvent"/> events of a chord's notes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Unable to get velocity since a chord doesn't contain notes.</description>
        /// </item>
        /// <item>
        /// <description>Unable to get velocity since chord's notes have different <see cref="Note.Velocity"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public SevenBitNumber Velocity
        {
            get
            {
                if (_velocity != null)
                    return _velocity.Value;

                SevenBitNumber? velocity = null;

                foreach (var note in Notes)
                {
                    if (velocity != null && note.Velocity != velocity)
                        throw new InvalidOperationException("Chord's notes have different velocities.");

                    velocity = note.Velocity;
                }

                if (velocity == null)
                    throw new InvalidOperationException("Chord is empty.");

                return (_velocity = velocity).Value;
            }
            set
            {
                foreach (var note in Notes)
                {
                    note.Velocity = value;
                }

                _velocity = value;
            }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOffEvent"/> events of a chord's notes.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Unable to get off velocity since a chord doesn't contain notes.</description>
        /// </item>
        /// <item>
        /// <description>Unable to get off velocity since chord's notes have different <see cref="Note.OffVelocity"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public SevenBitNumber OffVelocity
        {
            get
            {
                if (_offVelocity != null)
                    return _offVelocity.Value;

                SevenBitNumber? offVelocity = null;

                foreach (var note in Notes)
                {
                    if (offVelocity != null && note.OffVelocity != offVelocity)
                        throw new InvalidOperationException("Chord's notes have different off-velocities.");

                    offVelocity = note.OffVelocity;
                }

                if (offVelocity == null)
                    throw new InvalidOperationException("Chord is empty.");

                return (_offVelocity = offVelocity).Value;
            }
            set
            {
                foreach (var note in Notes)
                {
                    note.OffVelocity = value;
                }

                _offVelocity = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        public virtual ITimedObject Clone()
        {
            return new Chord(Notes.Select(note => (Note)note.Clone()));
        }

        /// <inheritdoc/>
        public SplitLengthedObject Split(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = Time;
            var endTime = startTime + Length;

            if (time <= startTime)
                return new SplitLengthedObject(null, (Chord)Clone());

            if (time >= endTime)
                return new SplitLengthedObject((Chord)Clone(), null);

            //

            var parts = Notes.Select(n => n.Split(time)).ToArray();

            var leftPart = new Chord(parts.Select(p => (Note)p.LeftPart).Where(p => p != null));
            var rightPart = new Chord(parts.Select(p => (Note)p.RightPart).Where(p => p != null));

            return new SplitLengthedObject(leftPart, rightPart);
        }

        internal void GetTimeAndLength(out long time, out long length)
        {
            var startTime = long.MaxValue;
            var endTime = long.MinValue;

            var hasNotes = false;

            foreach (var note in Notes)
            {
                var noteStartTime = note.Time;
                startTime = Math.Min(noteStartTime, startTime);

                var noteEndTime = noteStartTime + note.Length;
                endTime = Math.Max(noteEndTime, endTime);

                hasNotes = true;
            }

            time = hasNotes ? startTime : 0;
            length = hasNotes ? endTime - startTime : 0;
        }

        private void OnNotesCollectionChanged(TimedObjectsCollection<Note> collection, TimedObjectsCollectionChangedEventArgs<Note> args)
        {
            _channel = null;
            _velocity = null;
            _offVelocity = null;

            NotesCollectionChanged?.Invoke(collection, args);
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

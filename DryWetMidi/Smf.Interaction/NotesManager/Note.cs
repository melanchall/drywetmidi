using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a musical note.
    /// </summary>
    public sealed class Note : ILengthedObject, IEquatable<Note>
    {
        #region Constants

        private static readonly SevenBitNumber DefaultVelocity = (SevenBitNumber)100;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// note name and octave.
        /// </summary>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave in scientific pitch notation.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public Note(NoteName noteName, int octave)
            : this(noteName, octave, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// note name, octave and length.
        /// </summary>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave in scientific pitch notation.</param>
        /// <param name="length">Length of the note in units defined by time division of a MIDI file.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public Note(NoteName noteName, int octave, long length)
            : this(noteName, octave, length, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// note name, octave, length and absolute time.
        /// </summary>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave in scientific pitch notation.</param>
        /// <param name="length">Length of the note in units defined by time division of a MIDI file.</param>
        /// <param name="time">Absolute time of the note in units defined by the time division of a MIDI file.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public Note(NoteName noteName, int octave, long length, long time)
            : this(NoteUtilities.GetNoteNumber(noteName, octave))
        {
            Length = length;
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified note number.
        /// </summary>
        /// <param name="noteNumber">Number of the note (60 is middle C).</param>
        public Note(SevenBitNumber noteNumber)
            : this(noteNumber, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// note number and length.
        /// </summary>
        /// <param name="noteNumber">Number of the note (60 is middle C).</param>
        /// <param name="length">Length of the note in units defined by time division of a MIDI file.</param>
        public Note(SevenBitNumber noteNumber, long length)
            : this(noteNumber, length, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// note number, length and absolute time.
        /// </summary>
        /// <param name="noteNumber">Number of the note (60 is middle C).</param>
        /// <param name="length">Length of the note in units defined by time division of a MIDI file.</param>
        /// <param name="time">Absolute time of the note in units defined by the time division of a MIDI file.</param>
        public Note(SevenBitNumber noteNumber, long length, long time)
        {
            NoteNumber = noteNumber;
            Length = length;
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the specified
        /// Note On and Note Off timed events.
        /// </summary>
        /// <param name="timedNoteOnEvent">Wrapped <see cref="NoteOnEvent"/>.</param>
        /// <param name="timedNoteOffEvent">Wrapped <see cref="NoteOffEvent"/>.</param>
        internal Note(TimedEvent timedNoteOnEvent, TimedEvent timedNoteOffEvent)
        {
            Debug.Assert(timedNoteOnEvent != null);
            Debug.Assert(timedNoteOnEvent.Event is NoteOnEvent, "Timed event doesn't wrap a Note On event.");

            Debug.Assert(timedNoteOffEvent != null);
            Debug.Assert(timedNoteOffEvent.Event is NoteOffEvent, "Timed event doesn't wrap a Note Off event.");

            //

            var noteOnEvent = (NoteOnEvent)timedNoteOnEvent.Event;
            var noteOffEvent = (NoteOffEvent)timedNoteOffEvent.Event;

            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;

            NoteNumber = noteOnEvent.NoteNumber;
            Velocity = noteOnEvent.Velocity;
            OffVelocity = noteOffEvent.Velocity;
            Channel = noteOnEvent.Channel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets absolute time of the note in units defined by the time division of a MIDI file.
        /// </summary>
        public long Time
        {
            get { return TimedNoteOnEvent.Time; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                TimedNoteOffEvent.Time = value + Length;
                TimedNoteOnEvent.Time = value;
            }
        }

        /// <summary>
        /// Gets or sets length of the note in units defined by the time division of a MIDI file.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Length is negative.</exception>
        public long Length
        {
            get { return TimedNoteOffEvent.Time - TimedNoteOnEvent.Time; }
            set
            {
                ThrowIfLengthArgument.IsNegative(nameof(value), value);

                TimedNoteOffEvent.Time = TimedNoteOnEvent.Time + value;
            }
        }

        /// <summary>
        /// Gets or sets number of the note (60 is middle C).
        /// </summary>
        public SevenBitNumber NoteNumber { get; set; }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOnEvent"/>.
        /// </summary>
        public SevenBitNumber Velocity { get; set; } = DefaultVelocity;

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOffEvent"/>.
        /// </summary>
        public SevenBitNumber OffVelocity { get; set; }

        /// <summary>
        /// Gets or sets channel to play the note on.
        /// </summary>
        public FourBitNumber Channel { get; set; }

        /// <summary>
        /// Gets name of the note.
        /// </summary>
        public NoteName NoteName => NoteUtilities.GetNoteName(NoteNumber);

        /// <summary>
        /// Gets octave of the note.
        /// </summary>
        public int Octave => NoteUtilities.GetNoteOctave(NoteNumber);

        /// <summary>
        /// Gets Note On timed event of the note.
        /// </summary>
        internal TimedEvent TimedNoteOnEvent { get; } = new TimedEvent(new NoteOnEvent());

        /// <summary>
        /// Gets Note Off timed event of the note.
        /// </summary>
        internal TimedEvent TimedNoteOffEvent { get; } = new TimedEvent(new NoteOffEvent());

        #endregion

        #region Methods

        /// <summary>
        /// Sets note name and octave for current <see cref="Note"/>.
        /// </summary>
        /// <param name="noteName">Name of the note.</param>
        /// <param name="octave">Number of the octave in scientific pitch notation.</param>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the number of the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public void SetNoteNameAndOctave(NoteName noteName, int octave)
        {
            NoteNumber = NoteUtilities.GetNoteNumber(noteName, octave);
        }

        /// <summary>
        /// Determines whether the specified note is equal to the current one.
        /// </summary>
        /// <param name="note">The note to compare with the current one.</param>
        /// <param name="respectTime">If true the time will be taken into an account while comparing
        /// notes; if false - times will be ignored.</param>
        /// <param name="respectLength">If true the length will be taken into an account while comparing
        /// notes; if false - lengths will be ignored.</param>
        /// <returns>true if the specified note is equal to the current one; otherwise, false.</returns>
        public bool Equals(Note note, bool respectTime, bool respectLength)
        {
            if (ReferenceEquals(null, note))
                return false;

            if (ReferenceEquals(this, note))
                return true;

            return NoteNumber == note.NoteNumber &&
                   Channel == note.Channel &&
                   Velocity == note.Velocity &&
                   OffVelocity == note.OffVelocity &&
                   (!respectTime || Time == note.Time) &&
                   (!respectLength || Length == note.Length);
        }

        #endregion

        #region IEquatable<Note>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="note">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(Note note)
        {
            return Equals(note, true, true);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{NoteName.ToString().Replace("Sharp", "#")}{Octave} at {Time}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Note);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return NoteNumber.GetHashCode() ^
                   Channel.GetHashCode() ^
                   Velocity.GetHashCode() ^
                   OffVelocity.GetHashCode() ^
                   Time.GetHashCode() ^
                   Length.GetHashCode();
        }

        #endregion
    }
}

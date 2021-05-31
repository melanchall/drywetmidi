using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a musical note.
    /// </summary>
    public class Note : ILengthedObject, IMusicalObject, INotifyTimeChanged, INotifyLengthChanged
    {
        #region Constants

        /// <summary>
        /// Default velocity.
        /// </summary>
        public static readonly SevenBitNumber DefaultVelocity = (SevenBitNumber)100;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the time of an object has been changed.
        /// </summary>
        public event EventHandler<TimeChangedEventArgs> TimeChanged;

        /// <summary>
        /// Occurs when the length of an object has been changed.
        /// </summary>
        public event EventHandler<LengthChangedEventArgs> LengthChanged;

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
            var noteOnEvent = (NoteOnEvent)timedNoteOnEvent.Event;
            var noteOffEvent = (NoteOffEvent)timedNoteOffEvent.Event;

            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;

            Velocity = noteOnEvent.Velocity;
            OffVelocity = noteOffEvent.Velocity;
            Channel = noteOnEvent.Channel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets absolute time of the note in units defined by the time division of a MIDI file.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public long Time
        {
            get { return TimedNoteOnEvent.Time; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                var oldTime = Time;
                if (value == oldTime)
                    return;

                TimedNoteOffEvent.Time = value + Length;
                TimedNoteOnEvent.Time = value;

                TimeChanged?.Invoke(this, new TimeChangedEventArgs(oldTime, value));
            }
        }

        /// <summary>
        /// Gets or sets the length of the note in units defined by the time division of a MIDI file.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public long Length
        {
            get { return TimedNoteOffEvent.Time - TimedNoteOnEvent.Time; }
            set
            {
                ThrowIfLengthArgument.IsNegative(nameof(value), value);

                var oldLength = Length;
                if (value == oldLength)
                    return;

                TimedNoteOffEvent.Time = TimedNoteOnEvent.Time + value;

                LengthChanged?.Invoke(this, new LengthChangedEventArgs(oldLength, value));
            }
        }

        /// <summary>
        /// Gets or sets number of the note (60 is middle C).
        /// </summary>
        public SevenBitNumber NoteNumber
        {
            get { return ((NoteOnEvent)TimedNoteOnEvent.Event).NoteNumber; }
            set
            {
                ((NoteOnEvent)TimedNoteOnEvent.Event).NoteNumber = value;
                ((NoteOffEvent)TimedNoteOffEvent.Event).NoteNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOnEvent"/>.
        /// </summary>
        public SevenBitNumber Velocity
        {
            get { return ((NoteOnEvent)TimedNoteOnEvent.Event).Velocity; }
            set { ((NoteOnEvent)TimedNoteOnEvent.Event).Velocity = value; }
        }

        /// <summary>
        /// Gets or sets velocity of the underlying <see cref="NoteOffEvent"/>.
        /// </summary>
        public SevenBitNumber OffVelocity
        {
            get { return ((NoteOffEvent)TimedNoteOffEvent.Event).Velocity; }
            set { ((NoteOffEvent)TimedNoteOffEvent.Event).Velocity = value; }
        }

        /// <summary>
        /// Gets or sets channel to play the note on.
        /// </summary>
        public FourBitNumber Channel
        {
            get { return ((NoteOnEvent)TimedNoteOnEvent.Event).Channel; }
            set
            {
                ((NoteOnEvent)TimedNoteOnEvent.Event).Channel = value;
                ((NoteOffEvent)TimedNoteOffEvent.Event).Channel = value;
            }
        }

        /// <summary>
        /// Gets name of the note.
        /// </summary>
        public NoteName NoteName => ((NoteOnEvent)TimedNoteOnEvent.Event).GetNoteName();

        /// <summary>
        /// Gets octave of the note.
        /// </summary>
        public int Octave => ((NoteOnEvent)TimedNoteOnEvent.Event).GetNoteOctave();

        /// <summary>
        /// Gets Note On timed event of the note.
        /// </summary>
        internal TimedEvent TimedNoteOnEvent { get; } = new TimedEvent(new NoteOnEvent { Velocity = DefaultVelocity });

        /// <summary>
        /// Gets Note Off timed event of the note.
        /// </summary>
        internal TimedEvent TimedNoteOffEvent { get; } = new TimedEvent(new NoteOffEvent());

        internal MusicTheory.Note UnderlyingNote => MusicTheory.Note.Get(NoteNumber);

        #endregion

        #region Methods

        /// <summary>
        /// Gets the 'Note On' timed event of the current note.
        /// </summary>
        /// <returns>The 'Note On' timed event of the current note.</returns>
        public TimedEvent GetTimedNoteOnEvent()
        {
            return TimedNoteOnEvent.Clone();
        }

        /// <summary>
        /// Gets the 'Note Off' timed event of the current note.
        /// </summary>
        /// <returns>The 'Note Off' timed event of the current note.</returns>
        public TimedEvent GetTimedNoteOffEvent()
        {
            return TimedNoteOffEvent.Clone();
        }

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
        /// Clones note by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the note.</returns>
        public virtual Note Clone()
        {
            var newTimedNoteOnEvent = GetTimedNoteOnEvent();
            newTimedNoteOnEvent._time = TimedNoteOnEvent.Time;

            var newTimedNoteOffEvent = GetTimedNoteOffEvent();
            newTimedNoteOffEvent._time = TimedNoteOffEvent.Time;

            return new Note(newTimedNoteOnEvent, newTimedNoteOffEvent);
        }

        /// <summary>
        /// Splits the current <see cref="Note"/> by the specified time.
        /// </summary>
        /// <remarks>
        /// If <paramref name="time"/> is less than time of the note, the left part will be <c>null</c>.
        /// If <paramref name="time"/> is greater than end time of the note, the right part
        /// will be <c>null</c>.
        /// </remarks>
        /// <param name="time">Time to split the note by.</param>
        /// <returns>An object containing left and right parts of the split <see cref="Note"/>.
        /// Both parts are instances of <see cref="Note"/> too.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public SplitLengthedObject<Note> Split(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = Time;
            var endTime = startTime + Length;

            if (time <= startTime)
                return new SplitLengthedObject<Note>(null, Clone());

            if (time >= endTime)
                return new SplitLengthedObject<Note>(Clone(), null);

            //

            var leftPart = Clone();
            leftPart.Length = time - startTime;

            var rightPart = Clone();
            rightPart.Time = time;
            rightPart.Length = endTime - time;

            return new SplitLengthedObject<Note>(leftPart, rightPart);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return UnderlyingNote.ToString();
        }

        #endregion
    }
}

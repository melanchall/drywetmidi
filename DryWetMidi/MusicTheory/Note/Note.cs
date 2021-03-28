using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a note, i.e. note name and octave.
    /// </summary>
    public sealed class Note : IComparable<Note>
    {
        #region Constants

        internal const string SharpLongString = "Sharp";
        internal const string SharpShortString = "#";

        internal const string FlatLongString = "Flat";
        internal const string FlatShortString = "b";

        #endregion

        #region Fields

        private static readonly ConcurrentDictionary<SevenBitNumber, Note> Cache = new ConcurrentDictionary<SevenBitNumber, Note>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> with the
        /// specified note number.
        /// </summary>
        /// <param name="noteNumber">The number of a note (60 is middle C).</param>
        private Note(SevenBitNumber noteNumber)
        {
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the note number.
        /// </summary>
        public SevenBitNumber NoteNumber { get; }

        /// <summary>
        /// Gets the note name.
        /// </summary>
        public NoteName NoteName => NoteUtilities.GetNoteName(NoteNumber);

        /// <summary>
        /// Gets the octave number of a note.
        /// </summary>
        public int Octave => NoteUtilities.GetNoteOctave(NoteNumber);

        #endregion

        #region Methods

        /// <summary>
        /// Returns the current <see cref="Note"/> transposed by the specified
        /// <see cref="Interval"/>.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> to transpose the current
        /// <see cref="Note"/> by.</param>
        /// <returns>The current <see cref="Note"/> transposed by the <paramref name="interval"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Result note's number is out of valid range.</exception>
        public Note Transpose(Interval interval)
        {
            return Get((SevenBitNumber)(NoteNumber + interval.HalfSteps));
        }

        /// <summary>
        /// Returns a <see cref="Note"/> for the specified note number.
        /// </summary>
        /// <param name="noteNumber">The number of a note (60 is middle C).</param>
        /// <returns>A <see cref="Note"/> for the <paramref name="noteNumber"/>.</returns>
        public static Note Get(SevenBitNumber noteNumber)
        {
            Note note;
            if (!Cache.TryGetValue(noteNumber, out note))
                Cache.TryAdd(noteNumber, note = new Note(noteNumber));

            return note;
        }

        /// <summary>
        /// Returns a <see cref="Note"/> for the specified note name and octave number.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="octave">The octave number.</param>
        /// <returns>A <see cref="Note"/> for the <paramref name="noteName"/> and <paramref name="octave"/>.</returns>
        /// <remarks>
        /// Octave number is specified in scientific pitch notation which means that 4 must be
        /// passed to <paramref name="octave"/> to get the middle C.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an
        /// invalid value.</exception>
        /// <exception cref="ArgumentException">Note number is out of range for the specified note
        /// name and octave.</exception>
        public static Note Get(NoteName noteName, int octave)
        {
            return Get(NoteUtilities.GetNoteNumber(noteName, octave));
        }

        /// <summary>
        /// Converts the string representation of a musical note to its <see cref="Note"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a note to convert.</param>
        /// <param name="note">When this method returns, contains the <see cref="Note"/>
        /// equivalent of the musical note contained in <paramref name="input"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out Note note)
        {
            return ParsingUtilities.TryParse(input, NoteParser.TryParse, out note);
        }

        /// <summary>
        /// Converts the string representation of a musical note to its <see cref="Note"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a note to convert.</param>
        /// <returns>A <see cref="Note"/> equivalent to the musical note contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Note Parse(string input)
        {
            return ParsingUtilities.Parse<Note>(input, NoteParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Note"/> objects are equal.
        /// </summary>
        /// <param name="note1">The first <see cref="Note"/> to compare.</param>
        /// <param name="note2">The second <see cref="Note"/> to compare.</param>
        /// <returns><c>true</c> if the notes are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Note note1, Note note2)
        {
            if (ReferenceEquals(note1, note2))
                return true;

            if (ReferenceEquals(null, note1) || ReferenceEquals(null, note2))
                return false;

            return note1.NoteNumber == note2.NoteNumber;
        }

        /// <summary>
        /// Determines if two <see cref="Note"/> objects are not equal.
        /// </summary>
        /// <param name="note1">The first <see cref="Note"/> to compare.</param>
        /// <param name="note2">The second <see cref="Note"/> to compare.</param>
        /// <returns><c>false</c> if the notes are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(Note note1, Note note2)
        {
            return !(note1 == note2);
        }

        /// <summary>
        /// Transposes the specified <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to transpose.</param>
        /// <param name="halfSteps">The number of half steps to transpose the <paramref name="note"/> by.</param>
        /// <returns>The <see cref="Note"/> which is the <paramref name="note"/>
        /// transposed by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Result note's number is out of valid range.</exception>
        public static Note operator +(Note note, int halfSteps)
        {
            ThrowIfArgument.IsNull(nameof(note), note);

            return note.Transpose(Interval.FromHalfSteps(halfSteps));
        }

        /// <summary>
        /// Transposes the specified <see cref="Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to transpose.</param>
        /// <param name="halfSteps">The number of half steps to transpose the <paramref name="note"/> by.</param>
        /// <returns>The <see cref="Note"/> which is the <paramref name="note"/>
        /// transposed by the <paramref name="halfSteps"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Result note's number is out of valid range.</exception>
        public static Note operator -(Note note, int halfSteps)
        {
            return note + (-halfSteps);
        }

        #endregion

        #region IComparable<Note>

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        /// an integer that indicates whether the current instance precedes, follows, or
        /// occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns><para>A value that indicates the relative order of the objects being compared. The
        /// return value has these meanings:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>This instance precedes <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>This instance occurs in the same position in the sort order as <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>This instance follows <paramref name="other"/> in the sort order.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(Note other)
        {
            return NoteNumber.CompareTo(other.NoteNumber);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{NoteName.ToString().Replace(SharpLongString, SharpShortString)}{Octave}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Note);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return NoteNumber.GetHashCode();
        }

        #endregion
    }
}

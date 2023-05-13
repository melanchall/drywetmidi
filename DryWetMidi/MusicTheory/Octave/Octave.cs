using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents an octave defined by its number.
    /// </summary>
    public sealed class Octave
    {
        #region Fields

        private static readonly ConcurrentDictionary<int, Octave> Cache = new ConcurrentDictionary<int, Octave>();

        private readonly Dictionary<NoteName, Note> _notes;

        #endregion

        #region Constants

        /// <summary>
        /// The size of an octave, i.e. number of notes within an octave.
        /// </summary>
        public const int OctaveSize = 12;

        /// <summary>
        /// The smallest possible value of an octave's number.
        /// </summary>
        public static readonly int MinOctaveNumber = NoteUtilities.GetNoteOctave(SevenBitNumber.MinValue);

        /// <summary>
        /// The largest possible value of an octave's number.
        /// </summary>
        public static readonly int MaxOctaveNumber = NoteUtilities.GetNoteOctave(SevenBitNumber.MaxValue);

        /// <summary>
        /// The octave which contains the middle C note (C4).
        /// </summary>
        public static readonly Octave Middle = Get(4);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Octave"/> with the
        /// specified octave number.
        /// </summary>
        /// <param name="octave">The number of an octave.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octave"/> is out of valid range.</exception>
        private Octave(int octave)
        {
            Debug.Assert(octave >= MinOctaveNumber && octave <= MaxOctaveNumber,
                         "An octave's number is out of range.");

            Number = octave;

            _notes = Enum.GetValues(typeof(NoteName))
                         .Cast<NoteName>()
                         .Where(n => NoteUtilities.IsNoteValid(n, octave))
                         .ToDictionary(n => n,
                                       n => Note.Get(n, octave));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of an octave represented by the current <see cref="Octave"/>.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the C note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note C => GetNote(NoteName.C);

        /// <summary>
        /// Gets the C# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note CSharp => GetNote(NoteName.CSharp);

        /// <summary>
        /// Gets the D note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note D => GetNote(NoteName.D);

        /// <summary>
        /// Gets the D# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note DSharp => GetNote(NoteName.DSharp);

        /// <summary>
        /// Gets the E note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note E => GetNote(NoteName.E);

        /// <summary>
        /// Gets the F note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note F => GetNote(NoteName.F);

        /// <summary>
        /// Gets the F# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note FSharp => GetNote(NoteName.FSharp);

        /// <summary>
        /// Gets the G note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note G => GetNote(NoteName.G);

        /// <summary>
        /// Gets the G# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note GSharp => GetNote(NoteName.GSharp);

        /// <summary>
        /// Gets the A note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note A => GetNote(NoteName.A);

        /// <summary>
        /// Gets the A# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note ASharp => GetNote(NoteName.ASharp);

        /// <summary>
        /// Gets the B note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note.</exception>
        public Note B => GetNote(NoteName.B);

        #endregion

        #region Methods

        /// <summary>
        /// Gets a note by the specified note name using current octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <returns>Note with the specified note name and current octave.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Unable to get a note for the <paramref name="noteName"/>.</exception>
        public Note GetNote(NoteName noteName)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            Note note;
            if (!_notes.TryGetValue(noteName, out note))
                throw new InvalidOperationException($"Unable to get the {noteName} note.");

            return note;
        }

        /// <summary>
        /// Gets an octave by the specified octave number.
        /// </summary>
        /// <param name="octaveNumber">The number of an octave.</param>
        /// <returns>An octave with the specified number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octaveNumber"/> is out of valid range.</exception>
        public static Octave Get(int octaveNumber)
        {
            ThrowIfArgument.IsOutOfRange(nameof(octaveNumber),
                                         octaveNumber,
                                         MinOctaveNumber,
                                         MaxOctaveNumber,
                                         $"Octave number is out of [{MinOctaveNumber}, {MaxOctaveNumber}] range.");

            Octave octave;
            if (!Cache.TryGetValue(octaveNumber, out octave))
                Cache.TryAdd(octaveNumber, octave = new Octave(octaveNumber));

            return octave;
        }

        /// <summary>
        /// Converts the string representation of a musical octave to its <see cref="Octave"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing an octave to convert.</param>
        /// <param name="octave">When this method returns, contains the <see cref="Octave"/>
        /// equivalent of the musical octave contained in <paramref name="input"/>, if the conversion succeeded,
        /// or <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out Octave octave)
        {
            return ParsingUtilities.TryParse(input, OctaveParser.TryParse, out octave);
        }

        /// <summary>
        /// Converts the string representation of a musical octave to its <see cref="Octave"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing an octave to convert.</param>
        /// <returns>A <see cref="Octave"/> equivalent to the musical note contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Octave Parse(string input)
        {
            return ParsingUtilities.Parse<Octave>(input, OctaveParser.TryParse);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Octave"/> objects are equal.
        /// </summary>
        /// <param name="octave1">The first <see cref="Octave"/> to compare.</param>
        /// <param name="octave2">The second <see cref="Octave"/> to compare.</param>
        /// <returns><c>true</c> if the octaves are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Octave octave1, Octave octave2)
        {
            if (ReferenceEquals(octave1, octave2))
                return true;

            if (ReferenceEquals(null, octave1) || ReferenceEquals(null, octave2))
                return false;

            return octave1.Number == octave2.Number;
        }

        /// <summary>
        /// Determines if two <see cref="Octave"/> objects are not equal.
        /// </summary>
        /// <param name="octave1">The first <see cref="Octave"/> to compare.</param>
        /// <param name="octave2">The second <see cref="Octave"/> to compare.</param>
        /// <returns><c>false</c> if the octaves are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(Octave octave1, Octave octave2)
        {
            return !(octave1 == octave2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Octave {Number}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Octave);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        #endregion
    }
}

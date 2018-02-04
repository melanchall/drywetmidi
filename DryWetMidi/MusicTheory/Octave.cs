using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents an octave defined by its number.
    /// </summary>
    public sealed class Octave
    {
        #region Fields

        private static readonly Dictionary<int, Octave> _cache = new Dictionary<int, Octave>();

        private readonly Dictionary<NoteName, Note> _notesDefinitions;

        #endregion

        #region Constants

        /// <summary>
        /// The smalles possible value of an octave's number.
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

            _notesDefinitions = Enum.GetValues(typeof(NoteName))
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
        /// Gets the definition of the C note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note C => GetNoteDefinition(NoteName.C);

        /// <summary>
        /// Gets the definition of the C# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note CSharp => GetNoteDefinition(NoteName.CSharp);

        /// <summary>
        /// Gets the definition of the D note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note D => GetNoteDefinition(NoteName.D);

        /// <summary>
        /// Gets the definition of the D# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note DSharp => GetNoteDefinition(NoteName.DSharp);

        /// <summary>
        /// Gets the definition of the E note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note E => GetNoteDefinition(NoteName.E);

        /// <summary>
        /// Gets the definition of the F note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note F => GetNoteDefinition(NoteName.F);

        /// <summary>
        /// Gets the definition of the F# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note FSharp => GetNoteDefinition(NoteName.FSharp);

        /// <summary>
        /// Gets the definition of the G note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note G => GetNoteDefinition(NoteName.G);

        /// <summary>
        /// Gets the definition of the G# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note GSharp => GetNoteDefinition(NoteName.GSharp);

        /// <summary>
        /// Gets the definition of the A note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note A => GetNoteDefinition(NoteName.A);

        /// <summary>
        /// Gets the definition of the A# note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note ASharp => GetNoteDefinition(NoteName.ASharp);

        /// <summary>
        /// Gets the definition of the B note of an octave defined by the current <see cref="Octave"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public Note B => GetNoteDefinition(NoteName.B);

        #endregion

        #region Methods

        /// <summary>
        /// Gets a note definition by the specified note name using current octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <returns>Note definition which represents a note with the specified note name and
        /// current octave.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="InvalidOperationException">Unable to get a definition for the <paramref name="noteName"/>.</exception>
        public Note GetNoteDefinition(NoteName noteName)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            Note noteDefinition;
            if (!_notesDefinitions.TryGetValue(noteName, out noteDefinition))
                throw new InvalidOperationException($"Unable to get a definition for the {noteName} note.");

            return noteDefinition;
        }

        /// <summary>
        /// Gets an octave by the specified octave number.
        /// </summary>
        /// <param name="octaveNumber">The number of an octave.</param>
        /// <returns>A definition of the octave with the specified number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octaveNumber"/> is out of valid range.</exception>
        public static Octave Get(int octaveNumber)
        {
            ThrowIfArgument.IsOutOfRange(nameof(octaveNumber),
                                         octaveNumber,
                                         MinOctaveNumber,
                                         MaxOctaveNumber,
                                         $"Octave number is out of [{MinOctaveNumber}, {MaxOctaveNumber}] range.");

            Octave octave;
            if (!_cache.TryGetValue(octaveNumber, out octave))
                _cache.Add(octaveNumber, octave = new Octave(octaveNumber));

            return octave;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Octave"/> objects are equal.
        /// </summary>
        /// <param name="octaveDefinition1">The first <see cref="Octave"/> to compare.</param>
        /// <param name="octaveDefinition2">The second <see cref="Octave"/> to compare.</param>
        /// <returns>true if the octave definitions are equal, false otherwise.</returns>
        public static bool operator ==(Octave octaveDefinition1, Octave octaveDefinition2)
        {
            if (ReferenceEquals(octaveDefinition1, octaveDefinition2))
                return true;

            if (ReferenceEquals(null, octaveDefinition1) || ReferenceEquals(null, octaveDefinition2))
                return false;

            return octaveDefinition1.Number == octaveDefinition2.Number;
        }

        /// <summary>
        /// Determines if two <see cref="Octave"/> objects are not equal.
        /// </summary>
        /// <param name="octaveDefinition1">The first <see cref="Octave"/> to compare.</param>
        /// <param name="octaveDefinition2">The second <see cref="Octave"/> to compare.</param>
        /// <returns>false if the octave definitions are equal, true otherwise.</returns>
        public static bool operator !=(Octave octaveDefinition1, Octave octaveDefinition2)
        {
            return !(octaveDefinition1 == octaveDefinition2);
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
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
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

using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents an octave definition defined by its number.
    /// </summary>
    public sealed class OctaveDefinition
    {
        #region Constants

        /// <summary>
        /// The smalles possible value of an octave's number.
        /// </summary>
        public static readonly int MinOctave = NoteUtilities.GetNoteOctave(SevenBitNumber.MinValue);

        /// <summary>
        /// The largest possible value of an octave's number.
        /// </summary>
        public static readonly int MaxOctave = NoteUtilities.GetNoteOctave(SevenBitNumber.MaxValue);

        private static readonly Dictionary<int, OctaveDefinition> OctaveDefinitions =
            Enumerable.Range(MinOctave, MaxOctave - MinOctave + 1)
                      .ToDictionary(o => o,
                                    o => new OctaveDefinition(o));

        #endregion

        #region Fields

        private readonly Dictionary<NoteName, NoteDefinition> _notesDefinitions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="OctaveDefinition"/> with the
        /// specified octave number.
        /// </summary>
        /// <param name="octave">The number of an octave.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octave"/> is out of valid range.</exception>
        public OctaveDefinition(int octave)
        {
            ThrowIfArgument.IsOutOfRange(nameof(octave),
                                         octave,
                                         MinOctave,
                                         MaxOctave,
                                         $"Octave number is out of [{MinOctave}, {MaxOctave}] range.");

            _notesDefinitions = Enum.GetValues(typeof(NoteName))
                                    .Cast<NoteName>()
                                    .Where(n => NoteUtilities.IsNoteValid(n, octave))
                                    .ToDictionary(n => n,
                                                  n => new NoteDefinition(n, octave));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the definition of the C note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition C => GetNoteDefinition(NoteName.C);

        /// <summary>
        /// Gets the definition of the C# note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition CSharp => GetNoteDefinition(NoteName.CSharp);

        /// <summary>
        /// Gets the definition of the D note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition D => GetNoteDefinition(NoteName.D);

        /// <summary>
        /// Gets the definition of the D# note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition DSharp => GetNoteDefinition(NoteName.DSharp);

        /// <summary>
        /// Gets the definition of the E note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition E => GetNoteDefinition(NoteName.E);

        /// <summary>
        /// Gets the definition of the F note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition F => GetNoteDefinition(NoteName.F);

        /// <summary>
        /// Gets the definition of the F# note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition FSharp => GetNoteDefinition(NoteName.FSharp);

        /// <summary>
        /// Gets the definition of the G note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition G => GetNoteDefinition(NoteName.G);

        /// <summary>
        /// Gets the definition of the G# note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition GSharp => GetNoteDefinition(NoteName.GSharp);

        /// <summary>
        /// Gets the definition of the A note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition A => GetNoteDefinition(NoteName.A);

        /// <summary>
        /// Gets the definition of the A# note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition ASharp => GetNoteDefinition(NoteName.ASharp);

        /// <summary>
        /// Gets the definition of the B note of an octave defined by the current <see cref="OctaveDefinition"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to get a note definition.</exception>
        public NoteDefinition B => GetNoteDefinition(NoteName.B);

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
        public NoteDefinition GetNoteDefinition(NoteName noteName)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            NoteDefinition noteDefinition;
            if (!_notesDefinitions.TryGetValue(noteName, out noteDefinition))
                throw new InvalidOperationException($"Unable to get a definition for the {noteName} note.");

            return noteDefinition;
        }

        /// <summary>
        /// Gets an octave definition by the specified octave number.
        /// </summary>
        /// <param name="octave">The number of an octave.</param>
        /// <returns>A definition of the octave with the specified number.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="octave"/> is out of valid range.</exception>
        public static OctaveDefinition Get(int octave)
        {
            ThrowIfArgument.IsOutOfRange(nameof(octave),
                                         octave,
                                         MinOctave,
                                         MaxOctave,
                                         $"Octave number is out of [{MinOctave}, {MaxOctave}] range.");

            return OctaveDefinitions[octave];
        }

        #endregion
    }
}

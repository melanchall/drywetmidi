using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class OctaveDefinition
    {
        #region Constants

        public static readonly int MinOctave = NoteUtilities.GetNoteOctave(SevenBitNumber.MinValue);
        public static readonly int MaxOctave = NoteUtilities.GetNoteOctave(SevenBitNumber.MaxValue);

        #endregion

        #region Fields

        private readonly Dictionary<NoteName, NoteDefinition> _notesDefinitions;
        private static readonly Dictionary<int, OctaveDefinition> OctaveDefinitions = Enumerable.Range(MinOctave, MaxOctave - MinOctave + 1)
                                                                                                .ToDictionary(o => o,
                                                                                                              o => new OctaveDefinition(o));

        #endregion

        #region Constructor

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

        public NoteDefinition C => GetNoteDefinition(NoteName.C);

        public NoteDefinition CSharp => GetNoteDefinition(NoteName.CSharp);

        public NoteDefinition D => GetNoteDefinition(NoteName.D);

        public NoteDefinition DSharp => GetNoteDefinition(NoteName.DSharp);

        public NoteDefinition E => GetNoteDefinition(NoteName.E);

        public NoteDefinition F => GetNoteDefinition(NoteName.F);

        public NoteDefinition FSharp => GetNoteDefinition(NoteName.FSharp);

        public NoteDefinition G => GetNoteDefinition(NoteName.G);

        public NoteDefinition GSharp => GetNoteDefinition(NoteName.GSharp);

        public NoteDefinition A => GetNoteDefinition(NoteName.A);

        public NoteDefinition ASharp => GetNoteDefinition(NoteName.ASharp);

        public NoteDefinition B => GetNoteDefinition(NoteName.B);

        #endregion

        #region Methods

        public NoteDefinition GetNoteDefinition(NoteName noteName)
        {
            NoteDefinition noteDefinition;
            if (!_notesDefinitions.TryGetValue(noteName, out noteDefinition))
                throw new InvalidOperationException($"Unable to get a definition for the {noteName} note.");

            return noteDefinition;
        }

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

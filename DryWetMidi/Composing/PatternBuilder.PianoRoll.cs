using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Constants

        private static readonly char[] Digits = "0123456789".ToCharArray();

        #endregion

        #region Methods

        public PatternBuilder PianoRoll(
            string pianoRoll,
            PianoRollSettings settings = null)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(pianoRoll), pianoRoll, "Piano roll");

            var pattern = BuildPatternFromPianoRoll(
                pianoRoll,
                this,
                settings ?? new PianoRollSettings());

            return AddAction(new AddPatternAction(pattern));
        }

        private static Pattern BuildPatternFromPianoRoll(
            string pianoRoll,
            PatternBuilder parentPatternBuilder,
            PianoRollSettings settings)
        {
            var pianoRollStartSnchor = new object();
            var patternBuilder = new PatternBuilder()
                .SetNoteLength(parentPatternBuilder.NoteLength)
                .SetStep(parentPatternBuilder.NoteLength)
                .SetVelocity(parentPatternBuilder.Velocity)
                .Anchor(pianoRollStartSnchor);

            var lines = GetPianoRollLines(pianoRoll);

            foreach (var line in lines)
            {
                patternBuilder.MoveToLastAnchor(pianoRollStartSnchor);

                int dataStartIndex;
                var note = IdentifyLineNote(line, out dataStartIndex);

                ProcessLine(patternBuilder, settings, line, note, dataStartIndex);
            }

            return patternBuilder.Build();
        }

        private static MusicTheory.Note IdentifyLineNote(
            string line,
            out int dataStartIndex)
        {
            var digitIndex = line.IndexOfAny(Digits);
            var notePart = line.Substring(0, digitIndex + 1);

            MusicTheory.Note note;
            if (!MusicTheory.Note.TryParse(notePart, out note))
                throw new InvalidOperationException($"Failed to parse note from '{notePart}'.");

            dataStartIndex = digitIndex + 1;

            return note;
        }

        private static void ProcessLine(
            PatternBuilder patternBuilder,
            PianoRollSettings settings,
            string line,
            MusicTheory.Note note,
            int dataStartIndex)
        {
            var noteStartIndex = 0;
            var isNoteBuilding = false;

            for (var i = dataStartIndex; i < line.Length; i++)
            {
                var symbol = line[i];

                if (symbol == settings.SingleCellNoteSymbol)
                {
                    if (isNoteBuilding)
                        throw new InvalidOperationException("Single-cell note can't be placed inside a multi-cell one.");

                    patternBuilder.Note(note);
                }
                else if (symbol == settings.MultiCellNoteStartSymbol)
                {
                    if (isNoteBuilding)
                        throw new InvalidOperationException("Note can't be started while a previous one is not ended.");

                    isNoteBuilding = true;
                    noteStartIndex = i;
                }
                else if (symbol == settings.MultiCellNoteEndSymbol)
                {
                    if (!isNoteBuilding)
                        throw new InvalidOperationException("Note is not started.");

                    patternBuilder.Note(note, patternBuilder.NoteLength.Multiply(i - noteStartIndex + 1));
                    isNoteBuilding = false;
                }
                else
                {
                    Action<MusicTheory.Note, PatternBuilder> customAction;
                    if (settings.CustomActions?.TryGetValue(symbol, out customAction) == true)
                        customAction(note, patternBuilder);
                    else if (!isNoteBuilding)
                        patternBuilder.StepForward();
                }
            }
        }

        private static string[] GetPianoRollLines(string pianoRoll)
        {
            return pianoRoll
                .Split('\n', '\r')
                .Select(l => l.Trim().Replace(" ", string.Empty))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
        }

        #endregion
    }
}

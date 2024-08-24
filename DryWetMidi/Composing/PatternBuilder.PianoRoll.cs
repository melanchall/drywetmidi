using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Constants

        private static readonly char[] Digits = "0123456789".ToCharArray();

        #endregion

        #region Methods

        /// <summary>
        /// Inserts notes data by the specified piano roll string. More info in the
        /// <see href="xref:a_composing_pattern#piano-roll">Pattern: Piano roll</see> article.
        /// </summary>
        /// <param name="pianoRoll">String that represents notes data as a piano roll.</param>
        /// <param name="settings">Settings according to which a piano roll should be handled.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="pianoRoll"/> is <c>null</c>, a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="InvalidOperationException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>Failed to parse a note.</description>
        /// </item>
        /// <item>
        /// <description>Single-cell note can't be placed inside a multi-cell one.</description>
        /// </item>
        /// <item>
        /// <description>Note can't be started while a previous one is not ended.</description>
        /// </item>
        /// <item>
        /// <description>Note is not started.</description>
        /// </item>
        /// </list>
        /// </exception>
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
            var notePartEndIndex = line.IndexOfAny(Digits);
            var notePart = line.Substring(0, notePartEndIndex + 1);

            MusicTheory.Note note;
            if (!MusicTheory.Note.TryParse(notePart, out note))
            {
                notePartEndIndex = Enumerable.Range(0, line.Length).FirstOrDefault(i => !char.IsDigit(line[i])) - 1;
                notePart = line.Substring(0, notePartEndIndex + 1).Trim();

                SevenBitNumber noteNumber;
                if (!SevenBitNumber.TryParse(notePart, out noteNumber))
                    throw new InvalidOperationException($"Failed to parse a note from '{notePart}'.");
                else
                    note = MusicTheory.Note.Get(noteNumber);
            }

            dataStartIndex = notePartEndIndex + 1;

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
                    Action<MusicTheory.Note, PatternBuilder> customAction = null;
                    if (settings.CustomActions?.TryGetValue(symbol, out customAction) == true)
                        customAction?.Invoke(note, patternBuilder);
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

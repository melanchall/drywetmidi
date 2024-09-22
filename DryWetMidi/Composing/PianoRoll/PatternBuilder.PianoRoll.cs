using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Private enums

        private enum ActionInstruction
        {
            SingleCell,
            MultiCellStart,
            MultiCellEnd,
        }

        #endregion

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
                .SetOctave(parentPatternBuilder.Octave)
                .SetRootNote(parentPatternBuilder.RootNote)
                .Anchor(pianoRollStartSnchor);

            var lines = GetPianoRollLines(pianoRoll);
            var lineIndex = 0;

            foreach (var line in lines)
            {
                patternBuilder.MoveToLastAnchor(pianoRollStartSnchor);

                int dataStartIndex;
                var note = IdentifyLineNote(line, lineIndex, out dataStartIndex);

                ProcessLine(
                    patternBuilder,
                    settings,
                    line,
                    lineIndex++,
                    note,
                    dataStartIndex);
            }

            return patternBuilder.Build();
        }

        private static MusicTheory.Note IdentifyLineNote(
            string line,
            int lineIndex,
            out int dataStartIndex)
        {
            var notePartEndIndex = line.IndexOfAny(Digits);
            var notePart = line.Substring(0, notePartEndIndex + 1).Trim();

            MusicTheory.Note note;
            if (!MusicTheory.Note.TryParse(notePart, out note))
            {
                notePartEndIndex = Enumerable.Range(0, line.Length).FirstOrDefault(i => !char.IsDigit(line[i]) && !char.IsWhiteSpace(line[i])) - 1;
                notePart = line.Substring(0, notePartEndIndex + 1).Trim();

                SevenBitNumber noteNumber;
                if (!SevenBitNumber.TryParse(notePart, out noteNumber))
                    throw new InvalidOperationException($"Failed to parse a note from '{notePart}' (line {lineIndex}).");
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
            int lineIndex,
            MusicTheory.Note note,
            int dataStartIndex)
        {
            var multiCellActionStartIndex = 0;
            var multiCellActionInProgress = false;

            for (var symbolIndex = dataStartIndex; symbolIndex < line.Length; symbolIndex++)
            {
                var symbol = line[symbolIndex];
                if (char.IsWhiteSpace(symbol))
                    continue;

                var instruction = default(ActionInstruction?);
                var action = default(Action);

                if (symbol == settings.SingleCellNoteSymbol)
                {
                    instruction = ActionInstruction.SingleCell;
                    action = () => patternBuilder.Note(note);
                }
                else if (symbol == settings.MultiCellNoteStartSymbol)
                {
                    instruction = ActionInstruction.MultiCellStart;
                }
                else if (symbol == settings.MultiCellNoteEndSymbol)
                {
                    instruction = ActionInstruction.MultiCellEnd;
                    action = () => patternBuilder.Note(note, patternBuilder.NoteLength.Multiply(symbolIndex - multiCellActionStartIndex + 1));
                }
                else
                {
                    var customAction = settings
                        .CustomActions
                        ?.FirstOrDefault(a => a.StartSymbol == symbol || a.EndSymbol == symbol);

                    if (customAction != null)
                    {
                        if (customAction.EndSymbol != null)
                        {
                            if (customAction.StartSymbol != symbol || (customAction.EndSymbol == symbol && multiCellActionInProgress))
                            {
                                var cellsNumber = symbolIndex - multiCellActionStartIndex + 1;
                                instruction = ActionInstruction.MultiCellEnd;
                                action = () => customAction.Action(patternBuilder, new PianoRollActionContext(note, cellsNumber, patternBuilder.NoteLength.Multiply(cellsNumber)));
                            }
                            else if (customAction.StartSymbol == symbol && (customAction.EndSymbol != symbol || !multiCellActionInProgress))
                            {
                                instruction = ActionInstruction.MultiCellStart;
                            }
                        }
                        else
                        {
                            instruction = ActionInstruction.SingleCell;
                            action = () => customAction.Action(patternBuilder, new PianoRollActionContext(note, 1, patternBuilder.NoteLength));
                        }
                    }
                }

                switch (instruction)
                {
                    case ActionInstruction.SingleCell:
                        ExecuteSingleCellAction(
                            lineIndex,
                            symbolIndex,
                            multiCellActionInProgress,
                            action);
                        break;
                    case ActionInstruction.MultiCellStart:
                        StartMultiCellAction(
                            lineIndex,
                            symbolIndex,
                            ref multiCellActionStartIndex,
                            ref multiCellActionInProgress);
                        break;
                    case ActionInstruction.MultiCellEnd:
                        EndMultiCellAction(
                            lineIndex,
                            symbolIndex,
                            action,
                            ref multiCellActionInProgress);
                        break;
                    default:
                        if (!multiCellActionInProgress)
                            patternBuilder.StepForward();
                        break;
                }
            }
        }

        private static void ExecuteSingleCellAction(
            int lineIndex,
            int symbolIndex,
            bool multiCellActionInProgress,
            Action action)
        {
            if (multiCellActionInProgress)
                throw new InvalidOperationException($"Single-cell note can't be placed inside a multi-cell one (line {lineIndex}, position {symbolIndex}).");

            action();
        }

        private static void StartMultiCellAction(
            int lineIndex,
            int symbolIndex,
            ref int multiCellActionStartIndex,
            ref bool multiCellActionInProgress)
        {
            if (multiCellActionInProgress)
                throw new InvalidOperationException($"Note can't be started while a previous one is not ended (line {lineIndex}, position {symbolIndex}).");

            multiCellActionInProgress = true;
            multiCellActionStartIndex = symbolIndex;
        }

        private static void EndMultiCellAction(
            int lineIndex,
            int symbolIndex,
            Action action,
            ref bool multiCellActionInProgress)
        {
            if (!multiCellActionInProgress)
                throw new InvalidOperationException($"Note is not started (line {lineIndex}, position {symbolIndex}).");

            action();
            multiCellActionInProgress = false;
        }

        private static string[] GetPianoRollLines(string pianoRoll)
        {
            return pianoRoll
                .Split('\n', '\r')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
        }

        #endregion
    }
}

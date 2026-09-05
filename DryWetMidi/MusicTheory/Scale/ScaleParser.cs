using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class ScaleParser : SimpleParser<Scale>
    {
        private const string ScaleIsUnknown = "Scale is unknown.";

        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override Scale ParseInternal(ReadOnlySpan<char> input)
        {
            var (rootNoteName, rootNoteNamePartLength) = MusicTheoryParsers.NoteNameParser.TryReadNoteName(input);
            if (rootNoteName == null)
                ThrowInvalidFormatError();

            ICollection<Interval>? intervals = new List<Interval>();

            var (interval, intervalPartLength) = MusicTheoryParsers.IntervalParser.TryReadInterval(input.Slice(rootNoteNamePartLength).Trim());
            if (interval == null)
            {
                var rootNoteNameSlice = input.Slice(0, rootNoteNamePartLength);
                if (rootNoteNameSlice.EndsWith("b", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var n in ScaleIntervals.BNames)
                    {
                        if (input.Slice(rootNoteNamePartLength - 1).StartsWith(n, StringComparison.InvariantCultureIgnoreCase))
                        {
                            rootNoteName = (NoteName)(((int)rootNoteName.Value + 1) % Octave.OctaveSize);
                            rootNoteNamePartLength--;
                            break;
                        }
                    }
                }
                else if (rootNoteNameSlice.EndsWith("flat", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var n in ScaleIntervals.FlatNames)
                    {
                        if (input.Slice(rootNoteNamePartLength - 4).StartsWith(n, StringComparison.InvariantCultureIgnoreCase))
                        {
                            rootNoteName = (NoteName)(((int)rootNoteName.Value + 1) % Octave.OctaveSize);
                            rootNoteNamePartLength -= 4;
                            break;
                        }
                    }
                }

                var scaleName = input.Slice(rootNoteNamePartLength).Trim().ToString();
                intervals = ScaleIntervals.GetByName(scaleName);
            }
            else
            {
                var intervalsSlice = input.Slice(rootNoteNamePartLength).Trim();

                var i = 0;
                while (i < intervalsSlice.Length)
                {
                    (interval, intervalPartLength) = MusicTheoryParsers.IntervalParser.TryReadInterval(intervalsSlice.Slice(i).Trim());
                    if (interval == null)
                        ThrowInvalidFormatError();

                    intervals.Add(interval);
                    i += intervalPartLength;

                    while (i < intervalsSlice.Length && char.IsWhiteSpace(intervalsSlice[i]))
                    {
                        i++;
                    }
                }
            }

            if (intervals == null || !intervals.Any())
                ThrowError(ScaleIsUnknown);

            return new Scale(intervals, rootNoteName.Value);
        }
    }
}

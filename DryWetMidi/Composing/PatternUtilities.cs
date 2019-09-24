using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Composing
{
    public static class PatternUtilities
    {
        #region Methods

        public static Pattern TransformNotes(this Pattern pattern, NoteTransformation noteTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(noteTransformation), noteTransformation);

            return new Pattern(pattern.Actions.Select(a =>
            {
                var addNoteAction = a as AddNoteAction;
                if (addNoteAction != null)
                {
                    var noteDescriptor = noteTransformation(addNoteAction.NoteDescriptor);
                    return new AddNoteAction(noteDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    return new AddPatternAction(addPatternAction.Pattern.TransformNotes(noteTransformation));

                return a;
            })
            .ToList());
        }

        public static Pattern TransformChords(this Pattern pattern, ChordTransformation chordTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(chordTransformation), chordTransformation);

            return new Pattern(pattern.Actions.Select(a =>
            {
                var addChordAction = a as AddChordAction;
                if (addChordAction != null)
                {
                    var chordDescriptor = chordTransformation(addChordAction.ChordDescriptor);
                    return new AddChordAction(chordDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    return new AddPatternAction(addPatternAction.Pattern.TransformChords(chordTransformation));

                return a;
            })
            .ToList());
        }

        public static IEnumerable<Pattern> SplitAtAnchor(this Pattern pattern, object anchor, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            return SplitAtActions(
                pattern,
                a => (a as AddAnchorAction)?.Anchor == anchor,
                removeEmptyPatterns);
        }

        public static IEnumerable<Pattern> SplitAtAllAnchors(this Pattern pattern, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return SplitAtActions(
                pattern,
                a => a is AddAnchorAction,
                removeEmptyPatterns);
        }

        public static IEnumerable<Pattern> SplitAtMarker(this Pattern pattern, string marker, bool removeEmptyPatterns = true, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(marker), marker);

            return SplitAtActions(
                pattern,
                a => (a as AddTextEventAction<MarkerEvent>)?.Text.Equals(marker, stringComparison) == true,
                removeEmptyPatterns);
        }

        public static IEnumerable<Pattern> SplitAtAllMarkers(this Pattern pattern, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return SplitAtActions(
                pattern,
                a => a is AddTextEventAction<MarkerEvent>,
                removeEmptyPatterns);
        }

        public static Pattern CombineInSequence(this IEnumerable<Pattern> patterns)
        {
            ThrowIfArgument.IsNull(nameof(patterns), patterns);

            var patternBuilder = new PatternBuilder();

            foreach (var pattern in patterns.Where(p => p != null))
            {
                patternBuilder.Pattern(pattern);
            }

            return patternBuilder.Build();
        }

        public static Pattern CombineInParallel(this IEnumerable<Pattern> patterns)
        {
            ThrowIfArgument.IsNull(nameof(patterns), patterns);

            var patternBuilder = new PatternBuilder();

            foreach (var pattern in patterns.Where(p => p != null))
            {
                patternBuilder.Pattern(pattern).MoveToPreviousTime();
            }

            return patternBuilder.Build();
        }

        private static IEnumerable<Pattern> SplitAtActions(Pattern pattern, Predicate<IPatternAction> actionSelector, bool removeEmptyPatterns)
        {
            var part = new List<IPatternAction>();

            foreach (var action in pattern.Actions)
            {
                if (!actionSelector(action))
                {
                    part.Add(action);
                    continue;
                }

                if (part.Any() || !removeEmptyPatterns)
                    yield return new Pattern(part.AsReadOnly());

                part = new List<IPatternAction>();
            }

            if (part.Any())
                yield return new Pattern(part.AsReadOnly());
        }

        #endregion
    }
}

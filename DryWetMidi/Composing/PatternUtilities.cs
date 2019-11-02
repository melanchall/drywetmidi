using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Utilities to process an instance of the <see cref="Pattern"/>.
    /// </summary>
    public static class PatternUtilities
    {
        #region Methods

        /// <summary>
        /// Creates new <see cref="Pattern"/> by transforming notes in the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="noteTransformation">Transformation to apply to notes of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is true.</param>
        /// <returns><see cref="Pattern"/> that created by transforming notes of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="noteTransformation"/> is null.</exception>
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

        /// <summary>
        /// Creates new <see cref="Pattern"/> by transforming chords in the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="chordTransformation">Transformation to apply to chords of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is true.</param>
        /// <returns><see cref="Pattern"/> that created by transforming chords of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="chordTransformation"/> is null.</exception>
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

        /// <summary>
        /// Splits a pattern into subpatterns in points where the specified anchor inserted.
        /// </summary>
        /// <param name="pattern">Pattern to split.</param>
        /// <param name="anchor">Anchor to split <paramref name="pattern"/> at.</param>
        /// <param name="removeEmptyPatterns">A value indicating whether empty patterns should be
        /// automatically removed or not. The default value is true.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// splitted at <paramref name="anchor"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="anchor"/> is null.</exception>
        public static IEnumerable<Pattern> SplitAtAnchor(this Pattern pattern, object anchor, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            return SplitAtActions(
                pattern,
                a => (a as AddAnchorAction)?.Anchor == anchor,
                removeEmptyPatterns);
        }

        /// <summary>
        /// Splits a pattern into subpatterns in points where anchors inserted.
        /// </summary>
        /// <param name="pattern">Pattern to split.</param>
        /// <param name="removeEmptyPatterns">A value indicating whether empty patterns should be
        /// automatically removed or not. The default value is true.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// splitted at anchors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        public static IEnumerable<Pattern> SplitAtAllAnchors(this Pattern pattern, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return SplitAtActions(
                pattern,
                a => a is AddAnchorAction,
                removeEmptyPatterns);
        }

        /// <summary>
        /// Splits a pattern into subpatterns in points where the specified marker inserted.
        /// </summary>
        /// <param name="pattern">Pattern to split.</param>
        /// <param name="marker">Marker to split <paramref name="pattern"/> at.</param>
        /// <param name="removeEmptyPatterns">A value indicating whether empty patterns should be
        /// automatically removed or not. The default value is true.</param>
        /// <param name="stringComparison">Value that specifies how the marker strings will be compared.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// splitted at <paramref name="marker"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="marker"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="stringComparison"/> specified an invalid value.</exception>
        public static IEnumerable<Pattern> SplitAtMarker(this Pattern pattern, string marker, bool removeEmptyPatterns = true, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(marker), marker);
            ThrowIfArgument.IsInvalidEnumValue(nameof(stringComparison), stringComparison);

            return SplitAtActions(
                pattern,
                a => (a as AddTextEventAction<MarkerEvent>)?.Text.Equals(marker, stringComparison) == true,
                removeEmptyPatterns);
        }

        /// <summary>
        /// Splits a pattern into subpatterns in points where markers inserted.
        /// </summary>
        /// <param name="pattern">Pattern to split.</param>
        /// <param name="removeEmptyPatterns">A value indicating whether empty patterns should be
        /// automatically removed or not. The default value is true.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// splitted at markers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
        public static IEnumerable<Pattern> SplitAtAllMarkers(this Pattern pattern, bool removeEmptyPatterns = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);

            return SplitAtActions(
                pattern,
                a => a is AddTextEventAction<MarkerEvent>,
                removeEmptyPatterns);
        }

        /// <summary>
        /// Combines the specified patterns into single one placing them after each other.
        /// </summary>
        /// <param name="patterns">Patterns to combine.</param>
        /// <returns>Pattern that made up from <paramref name="patterns"/> following each other.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="patterns"/> is null.</exception>
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

        /// <summary>
        /// Combines the specified patterns into single one starting all them at the same time (i.e. stacking patterns).
        /// </summary>
        /// <param name="patterns">Patterns to combine.</param>
        /// <returns>Pattern that made up from <paramref name="patterns"/> arranged into stack.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="patterns"/> is null.</exception>
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

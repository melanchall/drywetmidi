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
        #region Constants

        private static readonly NoteSelection AllNotesSelection = (i, d) => true;
        private static readonly ChordSelection AllChordsSelection = (i, d) => true;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new <see cref="Pattern"/> by transforming notes in the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="noteTransformation">Transformation to apply to notes of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <returns><see cref="Pattern"/> that created by transforming notes of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="noteTransformation"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Pattern TransformNotes(this Pattern pattern, NoteTransformation noteTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(noteTransformation), noteTransformation);

            return TransformNotes(pattern, AllNotesSelection, noteTransformation, recursive);
        }

        /// <summary>
        /// Creates a new <see cref="Pattern"/> by transforming notes in the specified pattern using predicate
        /// to select notes to transform.
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="noteSelection">Predicate to select notes to transform.</param>
        /// <param name="noteTransformation">Transformation to apply to notes of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <returns><see cref="Pattern"/> that created by transforming notes of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="noteSelection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="noteTransformation"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Pattern TransformNotes(this Pattern pattern, NoteSelection noteSelection, NoteTransformation noteTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(noteSelection), noteSelection);
            ThrowIfArgument.IsNull(nameof(noteTransformation), noteTransformation);

            var noteIndexWrapper = new ObjectWrapper<int>();
            return TransformNotes(pattern, noteIndexWrapper, noteSelection, noteTransformation, recursive);
        }

        /// <summary>
        /// Creates a new <see cref="Pattern"/> by transforming chords in the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="chordTransformation">Transformation to apply to chords of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <returns><see cref="Pattern"/> that created by transforming chords of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="chordTransformation"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Pattern TransformChords(this Pattern pattern, ChordTransformation chordTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(chordTransformation), chordTransformation);

            return TransformChords(pattern, AllChordsSelection, chordTransformation, recursive);
        }

        /// <summary>
        /// Creates a new <see cref="Pattern"/> by transforming chords in the specified pattern using predicate
        /// to select chords to transform..
        /// </summary>
        /// <param name="pattern">Pattern to transform notes of.</param>
        /// <param name="chordSelection">Predicate to select chords to transform.</param>
        /// <param name="chordTransformation">Transformation to apply to chords of the <paramref name="pattern"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <returns><see cref="Pattern"/> that created by transforming chords of the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="chordSelection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="chordTransformation"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Pattern TransformChords(this Pattern pattern, ChordSelection chordSelection, ChordTransformation chordTransformation, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(chordSelection), chordSelection);
            ThrowIfArgument.IsNull(nameof(chordTransformation), chordTransformation);

            var chordIndexWrapper = new ObjectWrapper<int>();
            return TransformChords(pattern, chordIndexWrapper, chordSelection, chordTransformation, recursive);
        }

        /// <summary>
        /// Splits a pattern into subpatterns in points where the specified anchor inserted.
        /// </summary>
        /// <param name="pattern">Pattern to split.</param>
        /// <param name="anchor">Anchor to split <paramref name="pattern"/> at.</param>
        /// <param name="removeEmptyPatterns">A value indicating whether empty patterns should be
        /// automatically removed or not. The default value is <c>true</c>.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// split at <paramref name="anchor"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="anchor"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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
        /// automatically removed or not. The default value is <c>true</c>.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// split at anchors.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
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
        /// automatically removed or not. The default value is <c>true</c>.</param>
        /// <param name="stringComparison">Value that specifies how the marker strings will be compared.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// split at <paramref name="marker"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="marker"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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
        /// automatically removed or not. The default value is <c>true</c>.</param>
        /// <returns>A collection whose elements are the subpatterns from the <paramref name="pattern"/> that is
        /// split at markers.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="patterns"/> is <c>null</c>.</exception>
        public static Pattern MergeSequentially(this IEnumerable<Pattern> patterns)
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
        /// <exception cref="ArgumentNullException"><paramref name="patterns"/> is <c>null</c>.</exception>
        public static Pattern MergeSimultaneously(this IEnumerable<Pattern> patterns)
        {
            ThrowIfArgument.IsNull(nameof(patterns), patterns);

            var patternBuilder = new PatternBuilder();

            using (var enumerator = patterns.Where(p => p != null).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    patternBuilder.Pattern(enumerator.Current);

                    while (enumerator.MoveNext())
                        patternBuilder.MoveToPreviousTime().Pattern(enumerator.Current);
                }
            }

            return patternBuilder.Build();
        }

        /// <summary>
        /// Sets the state of notes within the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to set notes state within.</param>
        /// <param name="noteSelection">Predicate to select notes to set state.</param>
        /// <param name="state">State of notes selected with <paramref name="noteSelection"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="noteSelection"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="state"/> specified an invalid value.</exception>
        public static void SetNotesState(this Pattern pattern, NoteSelection noteSelection, PatternActionState state, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(noteSelection), noteSelection);
            ThrowIfArgument.IsInvalidEnumValue(nameof(state), state);

            var noteIndexWrapper = new ObjectWrapper<int>();
            SetNotesState(pattern, noteIndexWrapper, noteSelection, state, recursive);
        }

        /// <summary>
        /// Sets the state of chords within the specified pattern.
        /// </summary>
        /// <param name="pattern">Pattern to set chords state within.</param>
        /// <param name="chordSelection">Predicate to select chords to set state.</param>
        /// <param name="state">State of chords selected with <paramref name="chordSelection"/>.</param>
        /// <param name="recursive">A value indicating whether nested patterns should be processed or not. The
        /// default value is <c>true</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="chordSelection"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="state"/> specified an invalid value.</exception>
        public static void SetChordsState(this Pattern pattern, ChordSelection chordSelection, PatternActionState state, bool recursive = true)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(chordSelection), chordSelection);
            ThrowIfArgument.IsInvalidEnumValue(nameof(state), state);

            var chordIndexWrapper = new ObjectWrapper<int>();
            SetChordsState(pattern, chordIndexWrapper, chordSelection, state, recursive);
        }

        private static IEnumerable<Pattern> SplitAtActions(Pattern pattern, Predicate<PatternAction> actionSelector, bool removeEmptyPatterns)
        {
            var part = new List<PatternAction>();

            foreach (var action in pattern.Actions)
            {
                if (!actionSelector(action))
                {
                    part.Add(action);
                    continue;
                }

                if (part.Any() || !removeEmptyPatterns)
                    yield return new Pattern(part.AsReadOnly());

                part = new List<PatternAction>();
            }

            if (part.Any())
                yield return new Pattern(part.AsReadOnly());
        }

        private static Pattern TransformNotes(Pattern pattern, ObjectWrapper<int> noteIndexWrapper, NoteSelection noteSelection, NoteTransformation noteTransformation, bool recursive)
        {
            return new Pattern(pattern.Actions.Select(a =>
            {
                var addNoteAction = a as AddNoteAction;
                if (addNoteAction != null && noteSelection(noteIndexWrapper.Object++, addNoteAction.NoteDescriptor))
                {
                    var noteDescriptor = noteTransformation(addNoteAction.NoteDescriptor);
                    return new AddNoteAction(noteDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    return new AddPatternAction(TransformNotes(addPatternAction.Pattern, noteIndexWrapper, noteSelection, noteTransformation, recursive));

                return a.Clone();
            })
            .ToList());
        }

        private static Pattern TransformChords(Pattern pattern, ObjectWrapper<int> chordIndexWrapper, ChordSelection chordSelection, ChordTransformation chordTransformation, bool recursive)
        {
            return new Pattern(pattern.Actions.Select(a =>
            {
                var addChordAction = a as AddChordAction;
                if (addChordAction != null && chordSelection(chordIndexWrapper.Object++, addChordAction.ChordDescriptor))
                {
                    var chordDescriptor = chordTransformation(addChordAction.ChordDescriptor);
                    return new AddChordAction(chordDescriptor);
                }

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    return new AddPatternAction(TransformChords(addPatternAction.Pattern, chordIndexWrapper, chordSelection, chordTransformation, recursive));

                return a.Clone();
            })
            .ToList());
        }

        private static void SetNotesState(Pattern pattern, ObjectWrapper<int> noteIndexWrapper, NoteSelection noteSelection, PatternActionState state, bool recursive)
        {
            foreach (var a in pattern.Actions)
            {
                var addNoteAction = a as AddNoteAction;
                if (addNoteAction != null && noteSelection(noteIndexWrapper.Object++, addNoteAction.NoteDescriptor))
                    addNoteAction.State = state;

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    SetNotesState(addPatternAction.Pattern, noteIndexWrapper, noteSelection, state, recursive);
            }
        }

        private static void SetChordsState(Pattern pattern, ObjectWrapper<int> chordIndexWrapper, ChordSelection chordSelection, PatternActionState state, bool recursive)
        {
            foreach (var a in pattern.Actions)
            {
                var addChordAction = a as AddChordAction;
                if (addChordAction != null && chordSelection(chordIndexWrapper.Object++, addChordAction.ChordDescriptor))
                    addChordAction.State = state;

                var addPatternAction = a as AddPatternAction;
                if (addPatternAction != null && recursive)
                    SetChordsState(addPatternAction.Pattern, chordIndexWrapper, chordSelection, state, recursive);
            }
        }

        #endregion
    }
}

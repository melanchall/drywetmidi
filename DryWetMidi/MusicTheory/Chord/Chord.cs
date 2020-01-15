using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Represents a chord as a set of notes names.
    /// </summary>
    public sealed class Chord
    {
        #region Constants

        private static readonly Dictionary<ChordQuality, IntervalDefinition[]> IntervalsByQuality = new Dictionary<ChordQuality, IntervalDefinition[]>
        {
            [ChordQuality.Major] = new[] { new IntervalDefinition(3, IntervalQuality.Major), new IntervalDefinition(5, IntervalQuality.Perfect) },
            [ChordQuality.Minor] = new[] { new IntervalDefinition(3, IntervalQuality.Minor), new IntervalDefinition(5, IntervalQuality.Perfect) },
            [ChordQuality.Augmented] = new[] { new IntervalDefinition(3, IntervalQuality.Major), new IntervalDefinition(5, IntervalQuality.Augmented) },
            [ChordQuality.Diminished] = new[] { new IntervalDefinition(3, IntervalQuality.Minor), new IntervalDefinition(5, IntervalQuality.Diminished) }
        };

        private static readonly IEnumerable<Tuple<IEnumerable<IntervalDefinition>, Func<NoteName, ChordDefinition>>> KnownChordsDefinitionsCreators = IntervalsByQuality
            .SelectMany(definitions => new[] { 1, 6, 7, 9, 11, 13 }
                .SelectMany(extensionNumber => new[] { IntervalQuality.Minor, IntervalQuality.Major }
                    .SelectMany(seventhQuality => ((IntervalQuality[])Enum.GetValues(typeof(IntervalQuality)))
                        .SelectMany(extensionQuality => new int?[] { null, 2, 4 }
                            .SelectMany(suspensionNumber => new Accidental?[] { null, Accidental.Sharp, Accidental.Flat }
                                .Select(extensionAlteration => Tuple.Create<IEnumerable<IntervalDefinition>, Func<NoteName, ChordDefinition>>(
                                    GetChordIntervalsDefinitions(definitions.Value, new IntervalDefinition(extensionNumber, extensionQuality), extensionAlteration, suspensionNumber, seventhQuality),
                                    rootNoteName => new ChordDefinition(rootNoteName)
                                                    {
                                                        Quality = definitions.Key,
                                                        ExtensionInterval = extensionNumber > 1 ? new IntervalDefinition(extensionNumber, extensionQuality) : null,
                                                        ExtensionAlteration = extensionAlteration,
                                                        SuspensionIntervalNumber = suspensionNumber,
                                                        SeventhQuality = seventhQuality
                                                    })))))))
            .Where(chordDefinitionCreator => chordDefinitionCreator.Item1 != null)
            .ToArray();

        #endregion

        #region Fields

        private IReadOnlyCollection<ChordDefinition> _chordDefinitions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified notes names (for example, C E G).
        /// </summary>
        /// <param name="notesNames">The set of notes names.</param>
        /// <exception cref="ArgumentNullException"><paramref name="notesNames"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="notesNames"/> contains an invalid value.</exception>
        /// <exception cref="ArgumentException"><paramref name="notesNames"/> is empty collection.</exception>
        public Chord(ICollection<NoteName> notesNames)
        {
            ThrowIfArgument.IsNull(nameof(notesNames), notesNames);
            ThrowIfArgument.ContainsInvalidEnumValue(nameof(notesNames), notesNames);
            ThrowIfArgument.IsEmptyCollection(nameof(notesNames), notesNames, "Notes names collection is empty.");

            NotesNames = notesNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified root note name and
        /// names of the notes above root one (for example, C and E G).
        /// </summary>
        /// <param name="rootNoteName">The root note's name.</param>
        /// <param name="notesNamesAboveRoot">The set of names of the notes above root one.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value. -or-
        /// <paramref name="notesNamesAboveRoot"/> contains an invalid value.</exception>
        public Chord(NoteName rootNoteName, params NoteName[] notesNamesAboveRoot)
            : this(new[] { rootNoteName }.Concat(notesNamesAboveRoot ?? Enumerable.Empty<NoteName>()).ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified root note name and
        /// intervals from root one.
        /// </summary>
        /// <param name="rootNoteName">The root note's name.</param>
        /// <param name="intervalsFromRoot">Intervals from root note.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="intervalsFromRoot"/> is null.</exception>
        public Chord(NoteName rootNoteName, IEnumerable<Interval> intervalsFromRoot)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);
            ThrowIfArgument.IsNull(nameof(intervalsFromRoot), intervalsFromRoot);

            NotesNames = new[] { Interval.Zero }.Concat(intervalsFromRoot)
                .Where(i => i != null)
                .OrderBy(i => i.HalfSteps)
                .Select(i => rootNoteName.Transpose(i))
                .ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chord"/> with the specified root note name and
        /// intervals from root one.
        /// </summary>
        /// <param name="rootNoteName">The root note's name.</param>
        /// <param name="intervalsFromRoot">Intervals from root note.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="intervalsFromRoot"/> is null.</exception>
        public Chord(NoteName rootNoteName, params Interval[] intervalsFromRoot)
            : this(rootNoteName, intervalsFromRoot as IEnumerable<Interval>)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the chord's notes names.
        /// </summary>
        public ICollection<NoteName> NotesNames { get; }

        /// <summary>
        /// Gets the root note's name of the chord.
        /// </summary>
        public NoteName RootNoteName => NotesNames.First();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the collection of chord's inversions.
        /// </summary>
        /// <returns>Collection of chord's inversions.</returns>
        public IEnumerable<Chord> GetInversions()
        {
            var notesNames = new Queue<NoteName>(NotesNames);

            for (var i = 1; i < NotesNames.Count; i++)
            {
                var noteName = notesNames.Dequeue();
                notesNames.Enqueue(noteName);

                yield return new Chord(notesNames.ToArray());
            }
        }

        public IReadOnlyCollection<ChordDefinition> GetChordDefinitions()
        {
            if (_chordDefinitions != null)
                return _chordDefinitions;

            var definitions = GetChordDefinitionsInternal(NotesNames);
            var bassNoteName = NotesNames.First();

            foreach (var chordDefinition in GetChordDefinitionsInternal(NotesNames.Skip(1).ToArray()))
            {
                chordDefinition.BassNoteName = bassNoteName;
                definitions.Add(chordDefinition);
            }

            return _chordDefinitions = new ReadOnlyCollection<ChordDefinition>(definitions
                .Distinct()
                .Where(d => !d.AlteredIntervals.Any(i => i.IntervalNumber == d.ExtensionInterval?.Number ||
                                                         i.IntervalNumber == d.SuspensionIntervalNumber))
                .OrderBy(d => d.ToString().Length)
                .Take(10)
                .ToList());
        }

        /// <summary>
        /// Converts the string representation of a musical chord to its <see cref="Chord"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a chord to convert.</param>
        /// <param name="chord">When this method returns, contains the <see cref="Chord"/>
        /// equivalent of the musical chord contained in <paramref name="input"/>, if the conversion succeeded,
        /// or null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="string.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out Chord chord)
        {
            return ParsingUtilities.TryParse(input, ChordParser.TryParse, out chord);
        }

        /// <summary>
        /// Converts the string representation of a musical chord to its <see cref="Chord"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a chord to convert.</param>
        /// <returns>A <see cref="Chord"/> equivalent to the musical chord contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static Chord Parse(string input)
        {
            return ParsingUtilities.Parse<Chord>(input, ChordParser.TryParse);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Chord"/> as triad with possible addition of another notes by intervals
        /// from the root one.
        /// </summary>
        /// <param name="rootNoteName">The root note's name.</param>
        /// <param name="chordQuality">Chord's quality.</param>
        /// <param name="intervalsFromRoot">Intervals from root note.</param>
        /// <returns>Chord created by triad with addition of notes defined by <paramref name="intervalsFromRoot"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="rootNoteName"/> specified an invalid value. -or-
        /// <paramref name="chordQuality"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="intervalsFromRoot"/> is null.</exception>
        public static Chord GetByTriad(NoteName rootNoteName, ChordQuality chordQuality, params Interval[] intervalsFromRoot)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(rootNoteName), rootNoteName);
            ThrowIfArgument.IsInvalidEnumValue(nameof(chordQuality), chordQuality);
            ThrowIfArgument.IsNull(nameof(intervalsFromRoot), intervalsFromRoot);

            var intervals = IntervalsByQuality[chordQuality];
            return new Chord(rootNoteName, intervals.Select(i => Interval.FromDefinition(i)).Concat(intervalsFromRoot));
        }

        private static IList<ChordDefinition> GetChordDefinitionsInternal(ICollection<NoteName> notesNames)
        {
            if (notesNames.Count <= 1)
                return new List<ChordDefinition>();

            var rootNoteName = notesNames.First();
            var intervals = ChordUtilities.GetIntervalsFromRootNote(notesNames).ToArray();
            var result = new List<ChordDefinition>();

            // TODO: Get by cache
            var perfectFifthDefinition = new IntervalDefinition(5, IntervalQuality.Perfect);
            var perfectEightDefinition = new IntervalDefinition(8, IntervalQuality.Perfect);

            // Power chord

            if (notesNames.Count == 2)
            {
                if (intervals[0] == Interval.FromDefinition(perfectFifthDefinition))
                    result.Add(new ChordDefinition(rootNoteName) { ExtensionInterval = perfectFifthDefinition });
                else
                    return result;
            }

            if (notesNames.Count == 3 && intervals[0] == Interval.FromDefinition(perfectFifthDefinition) && intervals[1] == Interval.FromDefinition(perfectEightDefinition))
                result.Add(new ChordDefinition(rootNoteName) { ExtensionInterval = perfectFifthDefinition });

            //

            foreach (var chordDefinitionCreator in KnownChordsDefinitionsCreators)
            {
                var chordDefinition = chordDefinitionCreator.Item2(rootNoteName);
                if (CompleteChordDefinition(chordDefinition, intervals, chordDefinitionCreator.Item1))
                    result.Add(chordDefinition);
            }

            //

            return result;
        }

        private static bool CompleteChordDefinition(
            ChordDefinition chordDefinition,
            IEnumerable<Interval> intervals,
            IEnumerable<IntervalDefinition> referenceIntervalsDefinitions)
        {
            var intervalsList = new List<Interval>(intervals);
            var referenceIntervalsDefinitionsList = new List<IntervalDefinition>(referenceIntervalsDefinitions);

            foreach (var intervalDefinition in referenceIntervalsDefinitions)
            {
                if (!Interval.IsQualityApplicable(intervalDefinition.Quality, intervalDefinition.Number))
                    return false;

                var interval = Interval.FromDefinition(intervalDefinition);

                var nearestInterval = intervalsList.OrderBy(i => Math.Abs(i - interval)).FirstOrDefault();
                if (nearestInterval == null)
                    break;

                var offset = nearestInterval - interval;
                if (Math.Abs(offset) > 1)
                    continue;

                if (Math.Abs(offset) == 1)
                    chordDefinition.AlteredIntervals.Add(new ChordDefinition.IntervalAlteration(intervalDefinition.Number, offset > 0 ? Accidental.Sharp : Accidental.Flat));

                referenceIntervalsDefinitionsList.Remove(intervalDefinition);
                intervalsList.Remove(nearestInterval);
            }

            if (referenceIntervalsDefinitionsList.Any())
                return false;

            foreach (var interval in intervalsList)
            {
                var definition = interval.GetIntervalDefinitions().First();
                chordDefinition.AddedToneIntervals.Add(definition);
            }

            return true;
        }

        private static IEnumerable<IntervalDefinition> GetChordIntervalsDefinitions(
            IEnumerable<IntervalDefinition> baseIntervalsDefinitions,
            IntervalDefinition extensionIntervalDefinition,
            Accidental? extensionAlteration,
            int? suspensionNumber,
            IntervalQuality seventhQuality)
        {
            if (!Interval.IsQualityApplicable(extensionIntervalDefinition.Quality, extensionIntervalDefinition.Number))
                return null;

            var result = new List<IntervalDefinition>(baseIntervalsDefinitions);

            if (extensionIntervalDefinition.Number > 7)
            {
                result.Add(new IntervalDefinition(7, seventhQuality));

                for (var i = 9; i < extensionIntervalDefinition.Number; i += 2)
                {
                    result.Add(new IntervalDefinition(i, IntervalQuality.Major));
                }
            }

            if (extensionIntervalDefinition.Number > 1)
            {
                if (extensionAlteration == null)
                    result.Add(extensionIntervalDefinition);
                else
                    result.Add((Interval.FromDefinition(extensionIntervalDefinition) + (extensionAlteration == Accidental.Sharp ? 1 : -1)).GetIntervalDefinitions().First());
            }

            if (suspensionNumber != null)
                result[0] = suspensionNumber.Value == 2
                    ? new IntervalDefinition(2, IntervalQuality.Major)
                    : new IntervalDefinition(4, IntervalQuality.Perfect);

            return result;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Chord"/> objects are equal.
        /// </summary>
        /// <param name="chord1">The first <see cref="Chord"/> to compare.</param>
        /// <param name="chord2">The second <see cref="Chord"/> to compare.</param>
        /// <returns>true if the chords are equal, false otherwise.</returns>
        public static bool operator ==(Chord chord1, Chord chord2)
        {
            if (ReferenceEquals(chord1, chord2))
                return true;

            if (ReferenceEquals(null, chord1) || ReferenceEquals(null, chord2))
                return false;

            return chord1.NotesNames.SequenceEqual(chord2.NotesNames);
        }

        /// <summary>
        /// Determines if two <see cref="Chord"/> objects are not equal.
        /// </summary>
        /// <param name="chord1">The first <see cref="Chord"/> to compare.</param>
        /// <param name="chord2">The second <see cref="Chord"/> to compare.</param>
        /// <returns>false if the chords are equal, true otherwise.</returns>
        public static bool operator !=(Chord chord1, Chord chord2)
        {
            return !(chord1 == chord2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Join(" ", NotesNames.Select(n => n.ToString().Replace(Note.SharpLongString, Note.SharpShortString)));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Chord);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;

                foreach (var note in NotesNames)
                {
                    result = result * 23 + note.GetHashCode();
                }

                return result;
            }
        }

        #endregion
    }
}

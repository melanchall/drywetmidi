using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how chords should be detected and built.
    /// </summary>
    /// <seealso cref="ChordsManagingUtilities"/>
    public sealed class ChordDetectionSettings
    {
        #region Constants

        private const int DefaultNotesMinCount = 1;
        private static readonly long DefaultNotesTolerance = 0;

        #endregion

        #region Fields

        private int _notesMinCount = DefaultNotesMinCount;
        private long _notesTolerance = DefaultNotesTolerance;

        private ChordSearchContext _chordSearchContext = ChordSearchContext.SingleEventsCollection;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a minimum count of notes a chord can contain. So if count of simultaneously sounding
        /// notes is less than this value, they won't make up a chord. The default value is <c>1</c>.
        /// </summary>
        /// <example>
        /// <para>
        /// Let's take a look at the example:
        /// </para>
        /// <code language="image">
        /// ...[   ]...
        /// ...[     ].
        /// .....[   ].
        /// </code>
        /// <para>
        /// So we have three notes. For simplicity we'll assume that <see cref="NotesTolerance"/> is <c>0</c>
        /// (default value) and every character occupies 1 unit of time. If we set notes min count to <c>1</c>
        /// (which is default value), we'll get two different instances of <see cref="Chord"/>:
        /// </para>
        /// <code language="image">
        /// ...[111]...
        /// ...[11111].
        /// .....[222].
        /// </code>
        /// <para>
        /// <c>11..</c> and <c>22..</c> denote two different chords.
        /// </para>
        /// <para>
        /// If we set notes min count to <c>2</c>, we'll get only one chord:
        /// </para>
        /// <code language="image">
        /// ...[111]...
        /// ...[11111].
        ///        
        /// .....[   ]...
        /// </code>
        /// <para>
        /// Last note will not be turned into a chord because count of notes for a chord will be <c>1</c>
        /// which is less than the specified minimum count.
        /// </para>
        /// <para>
        /// With minimum count of notes of <c>3</c> we'll get no chords:
        /// </para>
        /// <code language="image">
        /// ...[   ]...
        /// ...[     ].
        /// .....[   ].
        /// </code>
        /// <para>
        /// First possible chord will contain two notes and second chord will contain one note. In both cases
        /// count of notes is less than the specified minimum count.
        /// </para>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int NotesMinCount
        {
            get { return _notesMinCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _notesMinCount = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum distance of notes from the start of the first note of a chord.
        /// Notes within this tolerance will be included in a chord. The default value is <c>0</c>.
        /// </summary>
        /// <example>
        /// <para>
        /// Let's take a look at the example:
        /// </para>
        /// <code language="image">
        /// ...[   ]...
        /// ....[     ]
        /// .....[   ].
        /// </code>
        /// <para>
        /// So we have three notes. Every character occupies 1 unit of time. If we set notes tolerance to
        /// <c>0</c> (which is default value), we'll get three different instances of <see cref="Chord"/>:
        /// </para>
        /// <code language="image">
        /// ...[111]...
        /// ....[22222]
        /// .....[333].
        /// </code>
        /// <para>
        /// <c>11..</c>, <c>22..</c> and <c>33..</c> denote three different chords.
        /// </para>
        /// <para>
        /// If we set notes tolerance to <c>1</c>, we'll get two chords:
        /// </para>
        /// <code language="image">
        /// ...[111]...
        /// ....[11111]
        /// .....[222].
        /// </code>
        /// <para>
        /// With tolerance of <c>2</c> we'll finally get a single chord:
        /// </para>
        /// <code language="image">
        /// ...[111]...
        /// ....[11111]
        /// .....[111].
        /// </code>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public long NotesTolerance
        {
            get { return _notesTolerance; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Value is negative.");

                _notesTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets settings according to which notes should be detected and built. Chords will be
        /// built on top of those notes.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        /// <summary>
        /// Gets or sets a value defining a context to search chords within. The default value is
        /// <see cref="ChordSearchContext.SingleEventsCollection"/>.
        /// </summary>
        /// <remarks>
        /// See Remarks section of the <see cref="Interaction.ChordSearchContext"/> enum.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ChordSearchContext ChordSearchContext
        {
            get { return _chordSearchContext; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _chordSearchContext = value;
            }
        }

        #endregion
    }
}

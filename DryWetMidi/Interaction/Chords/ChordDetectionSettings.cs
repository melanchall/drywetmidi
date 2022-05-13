using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how chords should be detected and built. More info in the
    /// <see href="xref:a_getting_objects#settings-1">Getting objects: GetChords: Settings</see> article.
    /// </summary>
    /// <seealso cref="ChordsManagingUtilities"/>
    /// <seealso cref="GetObjectsUtilities"/>
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
        /// Gets or sets custom construction method for <see cref="Chord"/>. If <c>null</c>,
        /// default method will be used (via one of the <see cref="Chord"/>'s constructors).
        /// </summary>
        public Func<ChordData, Chord> Constructor { get; set; }

        /// <summary>
        /// Gets or sets a minimum count of notes a chord can contain. So if count of simultaneously sounding
        /// notes is less than this value, they won't make up a chord. The default value is <c>1</c>. More info in the
        /// <see href="xref:a_getting_objects#notesmincount">Getting objects: GetChords: Settings: NotesMinCount</see>
        /// article.
        /// </summary>
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
        /// Notes within this tolerance will be included in a chord. The default value is <c>0</c>. More info in the
        /// <see href="xref:a_getting_objects#notestolerance">Getting objects: GetChords: Settings: NotesTolerance</see>
        /// article.
        /// </summary>
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
        /// built on top of those notes. More info in the
        /// <see href="xref:a_getting_objects#notedetectionsettings">Getting objects: GetChords: Settings: NoteDetectionSettings</see>
        /// article.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        /// <summary>
        /// Gets or sets a value defining a context to search chords within. The default value is
        /// <see cref="ChordSearchContext.SingleEventsCollection"/>. More info in the
        /// <see href="xref:a_getting_objects#chordsearchcontext">Getting objects: GetChords: Settings: ChordSearchContext</see>
        /// article.
        /// </summary>
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

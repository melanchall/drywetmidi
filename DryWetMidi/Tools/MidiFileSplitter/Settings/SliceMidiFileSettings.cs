using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which a <see cref="MidiFile"/> should be split.
    /// </summary>
    /// <seealso cref="MidiFileSplitter"/>
    public class SliceMidiFileSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether notes should be split in points of
        /// grid intersection or not. The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>false</c> means notes treated as just Note On / Note Off events rather than note objects
        /// for <c>true</c>. Splitting notes produces new Note On / Note Off events at points of grid
        /// intersecting notes.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// Take a look at the following file which should be split at the time defined by the vertical line:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +-------------║------------+
        /// |┌────────────║───────────┐|
        /// |│       [A    ║  B]        │
        /// |└───────⁞────║──⁞────────┘|
        /// |┌───────⁞────║──⁞────────┐|
        /// |│   [A   B]    ║  ⁞[  A ] B  │
        /// |└───⁞───⁞────║──⁞──⁞──⁞──┘|
        /// +----⁞---⁞----║--⁞--⁞--⁞---+
        /// </code>
        /// </para>
        /// <para>
        /// where <c>A</c> and <c>B</c> mean Note On and Note Off events correspondingly (notes are highlighted
        /// for convenience). So we have the file with two track chunks here containing some notes.
        /// </para>
        /// <para>
        /// If <see cref="SplitNotes"/> set to <c>false</c>, we'll get following two new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +----⁞---⁞-----+ ⁞  ⁞  ⁞
        /// |┌───⁞───⁞────┐| ⁞  ⁞  ⁞
        /// |│   ⁞   A    │| ⁞  ⁞  ⁞
        /// |└───⁞───⁞────┘| ⁞  ⁞  ⁞
        /// |┌───⁞───⁞────┐| ⁞  ⁞  ⁞
        /// |│   [A   B]    │  ⁞  ⁞  ⁞
        /// |└───⁞───⁞────┘| ⁞  ⁞  ⁞
        /// +----⁞---⁞-----+ ⁞  ⁞  ⁞
        ///      ⁞   ⁞   +---⁞--⁞--⁞---+
        ///      ⁞   ⁞   |┌──⁞──⁞──⁞──┐|
        ///      ⁞   ⁞   |│  B  ⁞  ⁞  │|
        ///      ⁞   ⁞   |└──⁞──⁞──⁞──┘|
        ///      ⁞   ⁞   |┌──⁞──⁞──⁞──┐|
        ///      ⁞   ⁞   |│  ⁞  [A  B]  │
        ///      ⁞   ⁞   |└──⁞──⁞──⁞──┘|
        ///      ⁞   ⁞   +---⁞--⁞--⁞---+
        /// </code>
        /// </para>
        /// <para>
        /// So the not within first track chunk just exploded into its Note On and Note Off events placed in
        /// different files after split. But if <see cref="SplitNotes"/> set to <c>true</c> (which is the default value),
        /// we'll get following new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +----⁞---⁞-----+ ⁞  ⁞  ⁞
        /// |┌───⁞───⁞────┐| ⁞  ⁞  ⁞
        /// |│   ⁞   [A    B]  ⁞  ⁞  ⁞
        /// |└───⁞───⁞────┘| ⁞  ⁞  ⁞
        /// |┌───⁞───⁞────┐| ⁞  ⁞  ⁞
        /// |│   [A   B]    │  ⁞  ⁞  ⁞
        /// |└────────────┘| ⁞  ⁞  ⁞
        /// +--------------+ ⁞  ⁞  ⁞
        ///              +---⁞--⁞--⁞---+
        ///              |┌──⁞──⁞──⁞──┐|
        ///               [A  B]  ⁞  ⁞  │
        ///              |└─────⁞──⁞──┘|
        ///              |┌─────⁞──⁞──┐|
        ///              |│     [A  B]  │
        ///              |└───────────┘|
        ///              +-------------+
        /// </code>
        /// </para>
        /// <para>
        /// So we have new Note On / Note Off events at points of split which gives us new notes.
        /// </para>
        /// </example>
        public bool SplitNotes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether original times of events should be saved or not.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If <c>false</c> used, events will be moved to the start of a new file. If <c>true</c> used, events
        /// will be placed in new files at the same absolute times as in the input file.
        /// </remarks>
        /// <example>
        /// <para>
        /// Take a look at the following file with two track chunks which should be split at the time
        /// defined by the vertical line:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +-------------║-----------+
        /// |┌────────────║──────────┐|
        /// |│     E      ║       E  │|
        /// |└─────⁞──────║───────⁞──┘|
        /// |┌─────⁞──────║───────⁞──┐|
        /// |│     ⁞    E ║ E     ⁞  │|
        /// |└─────⁞────⁞─║─⁞─────⁞──┘|
        /// +------⁞----⁞-║-⁞-----⁞---+
        /// </code>
        /// <para>
        /// where each <c>E</c> symbol represents a MIDI event.
        /// </para>
        /// </para>
        /// <para>
        /// If <see cref="PreserveTimes"/> set to <c>false</c> (which is the default value),
        /// we'll get following two new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +------⁞----⁞--+⁞     ⁞
        /// |┌─────⁞────⁞─┐|⁞     ⁞
        /// |│     E    ⁞ │|⁞     ⁞
        /// |└─────⁞────⁞─┘|⁞     ⁞
        /// |┌─────⁞────⁞─┐|⁞     ⁞
        /// |│     ⁞    E │|⁞     ⁞
        /// |└─────⁞────⁞─┘|⁞     ⁞
        /// +------⁞----⁞--+⁞     ⁞
        ///             ⁞+--⁞-----⁞---+
        ///             ⁞|┌─⁞─────⁞──┐|
        ///             ⁞|│ ⁞     E  │|
        ///             ⁞|└─⁞─────⁞──┘|
        ///             ⁞|┌─⁞─────⁞──┐|
        ///             ⁞|│ E     ⁞  │|
        ///             ⁞|└─⁞─────⁞──┘|
        ///             ⁞+--⁞-----⁞---+
        /// </code>
        /// </para>
        /// <para>
        /// But if <see cref="PreserveTimes"/> set to <c>true</c>, we'll get following new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +------⁞----⁞--+⁞     ⁞
        /// |┌─────⁞────⁞─┐|⁞     ⁞
        /// |│     E    ⁞ │|⁞     ⁞
        /// |└──────────⁞─┘|⁞     ⁞
        /// |┌──────────⁞─┐|⁞     ⁞
        /// |│          E │|⁞     ⁞
        /// |└────────────┘|⁞     ⁞
        /// +--------------+⁞     ⁞
        /// +---------------⁞-----⁞---+
        /// |┌──────────────⁞─────⁞──┐|
        /// |│              ⁞     E  │|
        /// |└──────────────⁞────────┘|
        /// |┌──────────────⁞────────┐|
        /// |│              E        │|
        /// |└───────────────────────┘|
        /// +-------------------------+
        /// </code>
        /// </para>
        /// <para>
        /// So events will not be shifted to the start of a file and will keep their original
        /// absolute times.
        /// </para>
        /// </example>
        public bool PreserveTimes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether track chunks in new files should correspond
        /// to those in the input file or not, so empty track chunks can be presented in new files.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <c>false</c> used, track chunks without events will be removed from the result.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// Take a look at the following file with two track chunks which should be split at the times
        /// defined by the vertical lines:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +-------------║-----║-----+
        /// |┌────────────║─────║────┐|
        /// |│     E      ║     ║  E │|
        /// |└─────⁞──────║─────║──⁞─┘|
        /// |┌─────⁞──────║─────║──⁞─┐|
        /// |│   E ⁞      ║ E   ║  ⁞ │|
        /// |└───⁞─⁞──────║─⁞───║──⁞─┘|
        /// +----⁞-⁞------║-⁞---║--⁞--+
        /// </code>
        /// </para>
        /// <para>
        /// where each <c>E</c> symbol represents a MIDI event.
        /// </para>
        /// <para>
        /// If <see cref="PreserveTrackChunks"/> set to <c>false</c> (which is the default value),
        /// we'll get following three new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +----⁞-⁞-------+⁞  +---⁞--+
        /// |┌───⁞─⁞──────┐|⁞  |┌──⁞─┐|
        /// |│   ⁞ E      │|⁞  |│  E │|
        /// |└───⁞─⁞──────┘|⁞  |└──⁞─┘|
        /// |┌───⁞─⁞──────┐|⁞  +---⁞--+
        /// |│   E ⁞      │|⁞      ⁞
        /// |└───⁞─⁞──────┘|⁞      ⁞
        /// +----⁞-⁞-------+⁞      ⁞
        ///      ⁞ ⁞     +--⁞---+  ⁞
        ///      ⁞ ⁞     |┌─⁞──┐|  ⁞
        ///      ⁞ ⁞     |│ E  │|  ⁞
        ///      ⁞ ⁞     |└─⁞──┘|  ⁞
        ///      ⁞ ⁞     +--⁞---+  ⁞
        /// </code>
        /// </para>
        /// <para>
        /// But if <see cref="PreserveTrackChunks"/> set to <c>true</c>, we'll get following new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +----⁞-⁞-------+⁞  +---⁞--+
        /// |┌───⁞─⁞──────┐|⁞  |┌──⁞─┐|
        /// |│   ⁞ E      │|⁞  |│  E │|
        /// |└───⁞────────┘|⁞  |└────┘|
        /// |┌───⁞────────┐|⁞  |┌────┐|
        /// |│   E        │|⁞  |│    │|
        /// |└────────────┘|⁞  |└────┘|
        /// +--------------+⁞  +------+
        ///              +--⁞---+ 
        ///              |┌─⁞──┐|
        ///              |│ ⁞  │|
        ///              |└─⁞──┘|
        ///              |┌─⁞──┐|
        ///              |│ E  │|
        ///              |└────┘|
        ///              +------+
        /// </code>
        /// </para>
        /// <para>
        /// So empty track chunks still present in new files because they're present in the original ones.
        /// </para>
        /// </example>
        public bool PreserveTrackChunks { get; set; } = false;

        /// <summary>
        /// Gets or sets <see cref="SliceMidiFileMarkers"/> that holds factory methods to create events
        /// to mark parts of split file.
        /// </summary>
        public SliceMidiFileMarkers Markers { get; set; }

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built. You can set it to
        /// <c>null</c> to use default settings.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; }

        #endregion
    }
}

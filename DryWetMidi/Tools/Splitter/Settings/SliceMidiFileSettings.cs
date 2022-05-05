using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which a <see cref="MidiFile"/> should be split by vertical
    /// split methods (for example, <see cref="Splitter.TakePart(MidiFile, ITimeSpan, SliceMidiFileSettings)"/>).
    /// </summary>
    /// <seealso cref="Splitter"/>
    public class SliceMidiFileSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether notes should be split in points of
        /// grid intersection or not. The default value is <c>true</c>.
        /// </summary>
        public bool SplitNotes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether original times of events should be saved or not.
        /// The default value is <c>false</c>.
        /// </summary>
        public bool PreserveTimes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether track chunks in new files should correspond
        /// to those in the input file or not, so empty track chunks can be presented in new files.
        /// The default value is <c>false</c>.
        /// </summary>
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

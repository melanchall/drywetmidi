using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which notes should be quantized.
    /// </summary>
    [Obsolete("OBS13")]
    public class NotesQuantizingSettings : LengthedObjectsQuantizingSettings<Note>
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        #endregion
    }
}

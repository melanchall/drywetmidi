namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class ObjectDetectionSettings
    {
        #region Properties

        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        public ChordDetectionSettings ChordDetectionSettings { get; set; } = new ChordDetectionSettings();

        public RestDetectionSettings RestDetectionSettings { get; set; } = new RestDetectionSettings();

        #endregion
    }
}

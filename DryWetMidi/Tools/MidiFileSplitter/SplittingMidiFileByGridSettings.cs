namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SplittingMidiFileByGridSettings
    {
        #region Properties

        public bool SplitNotes { get; set; } = true;

        public bool RemoveEmptyFiles { get; set; } = true;

        public bool PreserveTimes { get; set; } = false;

        public bool PreserveTrackChunks { get; set; } = false;

        #endregion
    }
}

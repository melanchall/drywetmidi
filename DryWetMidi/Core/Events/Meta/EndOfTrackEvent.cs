namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an End Of Track meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI end of track meta message denotes the end of a MIDI track.
    /// </remarks>
    public sealed class EndOfTrackEvent : MetaEvent
    {
        #region Constructor

        internal EndOfTrackEvent()
            : base(MidiEventType.EndOfTrack)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 0;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new EndOfTrackEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "End Of Track";
        }

        #endregion
    }
}

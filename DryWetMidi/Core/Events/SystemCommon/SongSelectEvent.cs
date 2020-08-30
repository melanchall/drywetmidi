using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Song Select event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI song request message (also known as a "song select message")
    /// tells a MIDI device to select a sequence for playback.
    /// </remarks>
    public sealed class SongSelectEvent : SystemCommonEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SongSelectEvent"/>.
        /// </summary>
        public SongSelectEvent()
            : base(MidiEventType.SongSelect)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SongSelectEvent"/> with the specified
        /// song number.
        /// </summary>
        /// <param name="number">Number of the song to be chosen.</param>
        public SongSelectEvent(SevenBitNumber number)
            : this()
        {
            Number = number;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets number of the song to be chosen.
        /// </summary>
        public SevenBitNumber Number { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            var number = reader.ReadByte();
            if (number > SevenBitNumber.MaxValue)
            {
                switch (settings.InvalidSystemCommonEventParameterValuePolicy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException(EventType, nameof(Number), number);
                    case InvalidSystemCommonEventParameterValuePolicy.SnapToLimits:
                        number = SevenBitNumber.MaxValue;
                        break;
                }
            }

            Number = (SevenBitNumber)number;
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Number);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SongSelectEvent(Number);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Song Number ({Number})";
        }

        #endregion
    }
}

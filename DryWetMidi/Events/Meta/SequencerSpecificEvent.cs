using System;

namespace Melanchall.DryWetMidi
{
    public sealed class SequencerSpecificEvent : MetaEvent
    {
        #region Constructor

        public SequencerSpecificEvent()
        {
        }

        public SequencerSpecificEvent(byte[] data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        #endregion

        #region Methods

        public bool Equals(SequencerSpecificEvent sequencerSpecificEvent)
        {
            return Equals(sequencerSpecificEvent, true);
        }

        public bool Equals(SequencerSpecificEvent sequencerSpecificEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sequencerSpecificEvent))
                return false;

            if (ReferenceEquals(this, sequencerSpecificEvent))
                return true;

            return base.Equals(sequencerSpecificEvent, respectDeltaTime) &&
                   ArrayUtilities.Equals(Data, sequencerSpecificEvent.Data);
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Sequencer Specific event.");

            Data = reader.ReadBytes(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        protected override int GetContentDataSize()
        {
            return Data?.Length ?? 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SequencerSpecificEvent(Data?.Clone() as byte[]);
        }

        public override string ToString()
        {
            return "Sequencer Specific";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SequencerSpecificEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}

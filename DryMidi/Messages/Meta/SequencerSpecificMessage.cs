using System;

namespace Melanchall.DryMidi
{
    public sealed class SequencerSpecificMessage : MetaMessage
    {
        #region Constructor

        public SequencerSpecificMessage()
        {
        }

        public SequencerSpecificMessage(byte[] data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        #endregion

        #region Methods

        public bool Equals(SequencerSpecificMessage sequencerSpecificMessage)
        {
            if (ReferenceEquals(null, sequencerSpecificMessage))
                return false;

            if (ReferenceEquals(this, sequencerSpecificMessage))
                return true;

            return base.Equals(sequencerSpecificMessage) && ArrayUtilities.Equals(Data, sequencerSpecificMessage.Data);
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Sequencer Specific message.");

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

        protected override Message CloneMessage()
        {
            return new SequencerSpecificMessage(Data?.Clone() as byte[]);
        }

        public override string ToString()
        {
            return "Sequencer Specific";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SequencerSpecificMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}

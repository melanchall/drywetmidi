using System;

namespace Melanchall.DryMidi
{
    public sealed class SetTempoMessage : MetaMessage
    {
        #region Constants

        public const long DefaultTempo = 500000;

        #endregion

        #region Fields

        private long _microsecondsPerBeat = DefaultTempo;

        #endregion

        #region Constructor

        public SetTempoMessage()
        {
        }

        public SetTempoMessage(long tempo)
            : this()
        {
            MicrosecondsPerBeat = tempo;
        }

        #endregion

        #region Properties

        public long MicrosecondsPerBeat
        {
            get { return _microsecondsPerBeat; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value of microseconds per beat is negative.", nameof(value));

                _microsecondsPerBeat = value;
            }
        }

        #endregion

        #region Methods

        public bool Equals(SetTempoMessage setTempoMessage)
        {
            if (ReferenceEquals(null, setTempoMessage))
                return false;

            if (ReferenceEquals(this, setTempoMessage))
                return true;

            return base.Equals(setTempoMessage) && MicrosecondsPerBeat == setTempoMessage.MicrosecondsPerBeat;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            MicrosecondsPerBeat = reader.Read3ByteDword();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.Write3ByteDword((uint)MicrosecondsPerBeat);
        }

        protected override int GetContentDataSize()
        {
            return 3;
        }

        protected override Message CloneMessage()
        {
            return new SetTempoMessage(MicrosecondsPerBeat);
        }

        public override string ToString()
        {
            return $"Set Tempo (tempo = {MicrosecondsPerBeat})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SetTempoMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ MicrosecondsPerBeat.GetHashCode();
        }

        #endregion
    }
}

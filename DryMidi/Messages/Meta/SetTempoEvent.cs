using System;

namespace Melanchall.DryMidi
{
    public sealed class SetTempoEvent : MetaEvent
    {
        #region Constants

        public const long DefaultTempo = 500000;

        #endregion

        #region Fields

        private long _microsecondsPerBeat = DefaultTempo;

        #endregion

        #region Constructor

        public SetTempoEvent()
        {
        }

        public SetTempoEvent(long tempo)
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

        public bool Equals(SetTempoEvent setTempoEvent)
        {
            if (ReferenceEquals(null, setTempoEvent))
                return false;

            if (ReferenceEquals(this, setTempoEvent))
                return true;

            return base.Equals(setTempoEvent) && MicrosecondsPerBeat == setTempoEvent.MicrosecondsPerBeat;
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

        protected override MidiEvent CloneEvent()
        {
            return new SetTempoEvent(MicrosecondsPerBeat);
        }

        public override string ToString()
        {
            return $"Set Tempo (tempo = {MicrosecondsPerBeat})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SetTempoEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ MicrosecondsPerBeat.GetHashCode();
        }

        #endregion
    }
}

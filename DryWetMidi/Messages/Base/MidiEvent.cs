using System;

namespace Melanchall.DryWetMidi
{
    public abstract class MidiEvent : ICloneable
    {
        #region Constants

        public const int UnknownContentSize = -1;

        #endregion

        #region Fields

        private int _deltaTime;

        #endregion

        #region Properties

        public int DeltaTime
        {
            get { return _deltaTime; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                                                          value,
                                                          "Delta-time have to be non-negative number.");

                _deltaTime = value;
            }
        }

        #endregion

        #region Methods

        internal abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size);

        internal abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        internal abstract int GetContentSize();

        protected abstract MidiEvent CloneEvent();

        public bool Equals(MidiEvent midiEvent)
        {
            if (ReferenceEquals(null, midiEvent))
                return false;

            if (ReferenceEquals(this, midiEvent))
                return true;

            return DeltaTime == midiEvent.DeltaTime;
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            var midiEvent = CloneEvent();
            midiEvent.DeltaTime = DeltaTime;
            return midiEvent;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MidiEvent);
        }

        public override int GetHashCode()
        {
            return DeltaTime.GetHashCode();
        }

        #endregion
    }
}

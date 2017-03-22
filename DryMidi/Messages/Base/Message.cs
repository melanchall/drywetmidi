using System;

namespace Melanchall.DryMidi
{
    public abstract class Message : ICloneable
    {
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Delta-time have to be non-negative number.");

                _deltaTime = value;
            }
        }

        #endregion

        #region Methods

        public abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1);

        public abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        public abstract int GetContentSize();

        protected abstract Message CloneMessage();

        #endregion

        #region ICloneable

        public object Clone()
        {
            var message = CloneMessage();
            message.DeltaTime = DeltaTime;
            return message;
        }

        #endregion
    }
}

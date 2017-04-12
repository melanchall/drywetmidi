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
                    throw new ArgumentOutOfRangeException(nameof(value),
                                                          value,
                                                          "Delta-time have to be non-negative number.");

                _deltaTime = value;
            }
        }

        #endregion

        #region Methods

        internal abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1);

        internal abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        internal abstract int GetContentSize();

        protected abstract Message CloneMessage();

        public bool Equals(Message message)
        {
            if (ReferenceEquals(null, message))
                return false;

            if (ReferenceEquals(this, message))
                return true;

            return DeltaTime == message.DeltaTime;
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            var message = CloneMessage();
            message.DeltaTime = DeltaTime;
            return message;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as Message);
        }

        public override int GetHashCode()
        {
            return DeltaTime.GetHashCode();
        }

        #endregion
    }
}

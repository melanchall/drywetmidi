using System;
using System.Linq;

namespace Melanchall.DryMidi
{
    public abstract class ChannelEvent : MidiEvent
    {
        #region Fields

        protected readonly SevenBitNumber[] _parameters;

        #endregion

        #region Constructor

        protected ChannelEvent(int parametersCount)
        {
            if (parametersCount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(parametersCount),
                    parametersCount,
                    "Parameters count have to be non-negative number.");

            _parameters = new SevenBitNumber[parametersCount];
        }

        #endregion

        #region Properties

        public FourBitNumber Channel { get; set; }

        #endregion

        #region Methods

        public bool Equals(ChannelEvent channelEvent)
        {
            if (ReferenceEquals(null, channelEvent))
                return false;

            if (ReferenceEquals(this, channelEvent))
                return true;

            return base.Equals(channelEvent) && _parameters.SequenceEqual(channelEvent._parameters);
        }

        #endregion

        #region Overrides

        internal override sealed void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            for (int i = 0; i < _parameters.Length; i++)
            {
                _parameters[i] = (SevenBitNumber)reader.ReadByte();
            }
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            foreach (var parameter in _parameters)
            {
                writer.WriteByte(parameter);
            }
        }

        internal override int GetContentSize()
        {
            return _parameters.Length;
        }

        protected sealed override MidiEvent CloneEvent()
        {
            var eventType = GetType();
            var channelEvent = (ChannelEvent)Activator.CreateInstance(eventType);

            channelEvent.Channel = Channel;
            Array.Copy(_parameters, channelEvent._parameters, _parameters.Length);

            return channelEvent;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ChannelEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _parameters.GetHashCode();
        }

        #endregion
    }
}

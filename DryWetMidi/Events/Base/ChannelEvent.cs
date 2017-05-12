using System;
using System.Linq;

namespace Melanchall.DryWetMidi
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
            return Equals(channelEvent, true);
        }

        public bool Equals(ChannelEvent channelEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, channelEvent))
                return false;

            if (ReferenceEquals(this, channelEvent))
                return true;

            return base.Equals(channelEvent, respectDeltaTime) && _parameters.SequenceEqual(channelEvent._parameters);
        }

        #endregion

        #region Overrides

        internal override sealed void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            for (int i = 0; i < _parameters.Length; i++)
            {
                var parameter = reader.ReadByte();
                if (parameter > SevenBitNumber.MaxValue)
                {
                    switch (settings.InvalidChannelEventParameterValuePolicy)
                    {
                        case InvalidChannelEventParameterValuePolicy.Abort:
                            throw new InvalidChannelEventParameterValueException($"{parameter} is invalid value for channel event's parameter.", parameter);
                        case InvalidChannelEventParameterValuePolicy.ReadValid:
                            parameter &= 127;
                            break;
                    }
                }

                _parameters[i] = (SevenBitNumber)parameter;
            }
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            foreach (var parameter in _parameters)
            {
                writer.WriteByte(parameter);
            }
        }

        internal override int GetSize()
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

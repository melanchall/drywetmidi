using System;

namespace Melanchall.DryMidi
{
    public abstract class ChannelMessage : Message
    {
        #region Fields

        protected readonly SevenBitNumber[] _parameters;

        #endregion

        #region Constructor

        protected ChannelMessage(int parametersCount)
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

        #region Overrides

        internal override sealed void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
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

        protected sealed override Message CloneMessage()
        {
            var messageType = GetType();
            var message = (ChannelMessage)Activator.CreateInstance(messageType);

            message.Channel = Channel;
            Array.Copy(_parameters, message._parameters, _parameters.Length);

            return message;
        }

        #endregion
    }
}

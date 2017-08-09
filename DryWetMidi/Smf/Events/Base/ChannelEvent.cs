using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a MIDI file channel event.
    /// </summary>
    public abstract class ChannelEvent : MidiEvent
    {
        #region Fields

        /// <summary>
        /// Parameters of the MIDI channel event.
        /// </summary>
        private readonly SevenBitNumber[] _parameters;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEvent"/> with the specified parameters count.
        /// </summary>
        /// <param name="parametersCount">Count of the parameters for this channel event.</param>
        /// <exception cref="ArgumentOutOfRangeException">Parameters count is negative number which is unallowable.</exception>
        protected ChannelEvent(int parametersCount)
        {
            ThrowIfArgument.IsNegative(nameof(parametersCount),
                                        parametersCount,
                                        "Parameters count is negative.");

            _parameters = new SevenBitNumber[parametersCount];
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets channel for this event.
        /// </summary>
        public FourBitNumber Channel { get; set; }

        /// <summary>
        /// Gets or sets the parameter's value at the specified index.
        /// </summary>
        /// <param name="index">Index of the parameter.</param>
        /// <returns>Value of parameter at the specified index.</returns>
        protected SevenBitNumber this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="InvalidChannelEventParameterValueException">An invalid value for channel
        /// event's parameter was encountered.</exception>
        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
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
                            parameter &= SevenBitNumber.MaxValue;
                            break;
                        case InvalidChannelEventParameterValuePolicy.SnapToLimits:
                            parameter = SevenBitNumber.MaxValue;
                            break;
                    }
                }

                _parameters[i] = (SevenBitNumber)parameter;
            }
        }

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            foreach (var parameter in _parameters)
            {
                writer.WriteByte(parameter);
            }
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize(WritingSettings settings)
        {
            return _parameters.Length;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected sealed override MidiEvent CloneEvent()
        {
            var eventType = GetType();
            var channelEvent = (ChannelEvent)Activator.CreateInstance(eventType);

            channelEvent.Channel = Channel;
            Array.Copy(_parameters,
                       channelEvent._parameters,
                       _parameters.Length);

            return channelEvent;
        }

        #endregion
    }
}

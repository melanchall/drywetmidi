using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class MidiTimeCodeEvent : SystemCommonEvent
    {
        #region Constants

        private static readonly Dictionary<MidiTimeCodeComponent, byte> ComponentValueMasks = new Dictionary<MidiTimeCodeComponent, byte>
        {
            [MidiTimeCodeComponent.FramesLsb] = 0xF,
            [MidiTimeCodeComponent.FramesMsb] = 0x1,
            [MidiTimeCodeComponent.SecondsLsb] = 0xF,
            [MidiTimeCodeComponent.SecondsMsb] = 0x3,
            [MidiTimeCodeComponent.MinutesLsb] = 0xF,
            [MidiTimeCodeComponent.MinutesMsb] = 0x3,
            [MidiTimeCodeComponent.HoursLsb] = 0xF,
            [MidiTimeCodeComponent.HoursMsbAndTimeCodeType] = 0x7
        };

        #endregion

        #region Constructor

        public MidiTimeCodeEvent()
        {
        }

        public MidiTimeCodeEvent(MidiTimeCodeComponent component, FourBitNumber componentValue)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(component), component);

            Component = component;
            ComponentValue = componentValue;
        }

        #endregion

        #region Properties

        public MidiTimeCodeComponent Component { get; set; }

        public FourBitNumber ComponentValue { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            var data = reader.ReadByte();

            var messageType = data.GetHead();
            // TODO: proper exception
            if (!Enum.IsDefined(typeof(MidiTimeCodeComponent), (byte)messageType))
                throw new Exception();

            Component = (MidiTimeCodeComponent)(byte)messageType;

            var componentValue = data.GetTail();
            if (componentValue > ComponentValueMasks[Component])
            {
                switch (settings.InvalidSystemCommonEventParameterValuePolicy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException($"{componentValue} is invalid value for the {nameof(ComponentValue)} of {Component} of a MIDI Time Code event.", componentValue);
                    case InvalidSystemCommonEventParameterValuePolicy.SnapToLimits:
                        componentValue = (FourBitNumber)ComponentValueMasks[Component];
                        break;
                }
            }

            ComponentValue = componentValue;
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            var component = Component;

            var componentValueMask = ComponentValueMasks[component];
            var componentValue = ComponentValue & componentValueMask;

            var data = DataTypesUtilities.Combine((FourBitNumber)(byte)component,
                                                  (FourBitNumber)(byte)componentValue);

            writer.WriteByte(data);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        protected override MidiEvent CloneEvent()
        {
            return new MidiTimeCodeEvent(Component, ComponentValue);
        }

        public override string ToString()
        {
            return $"MIDI Time Code ({Component}, {ComponentValue})";
        }

        #endregion
    }
}

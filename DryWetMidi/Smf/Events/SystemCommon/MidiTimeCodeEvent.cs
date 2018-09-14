using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class MidiTimeCodeEvent : SystemCommonEvent
    {
        #region Constants

        private static readonly Dictionary<MidiTimeCodeEventComponent, byte> ComponentValueMasks = new Dictionary<MidiTimeCodeEventComponent, byte>
        {
            [MidiTimeCodeEventComponent.FramesLsb] = 0xF,
            [MidiTimeCodeEventComponent.FramesMsb] = 0x1,
            [MidiTimeCodeEventComponent.SecondsLsb] = 0xF,
            [MidiTimeCodeEventComponent.SecondsMsb] = 0x3,
            [MidiTimeCodeEventComponent.MinutesLsb] = 0xF,
            [MidiTimeCodeEventComponent.MinutesMsb] = 0x3,
            [MidiTimeCodeEventComponent.HoursLsb] = 0xF,
            [MidiTimeCodeEventComponent.HoursMsbAndTimeCodeType] = 0x7
        };

        #endregion

        #region Constructor

        public MidiTimeCodeEvent()
        {
        }

        public MidiTimeCodeEvent(MidiTimeCodeEventComponent component, FourBitNumber componentValue)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(component), component);

            Component = component;
            ComponentValue = componentValue;
        }

        #endregion

        #region Properties

        public MidiTimeCodeEventComponent Component { get; set; }

        public FourBitNumber ComponentValue { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            var data = reader.ReadByte();

            var messageType = data.GetHead();
            // TODO: proper exception
            if (!Enum.IsDefined(typeof(MidiTimeCodeEventComponent), (byte)messageType))
                throw new Exception();

            Component = (MidiTimeCodeEventComponent)(byte)messageType;
            // TODO: check value and apply policy
            ComponentValue = data.GetTail();
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents MIDI Time Code (MIDI Quarter Frame) event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI quarter frame message is timing information in the
    /// hours:minutes:seconds:frames format (similar to SMPTE) that is used to synchronize MIDI devices.
    /// </remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiTimeCodeEvent"/>.
        /// </summary>
        public MidiTimeCodeEvent()
            : base(MidiEventType.MidiTimeCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiTimeCodeEvent"/> with the specified
        /// time code component and its value.
        /// </summary>
        /// <param name="component">MIDI time code component.</param>
        /// <param name="componentValue">Value of <paramref name="component"/>.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="component"/> specified an
        /// invalid value.</exception>
        public MidiTimeCodeEvent(MidiTimeCodeComponent component, FourBitNumber componentValue)
            : this()
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(component), component);

            var maximumComponentValue = ComponentValueMasks[component];
            ThrowIfArgument.IsGreaterThan(nameof(componentValue),
                                          componentValue,
                                          maximumComponentValue,
                                          $"Component's value is greater than maximum valid one which is {maximumComponentValue}.");

            Component = component;
            ComponentValue = componentValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the MIDI time code component presented by the current <see cref="MidiTimeCodeEvent"/>.
        /// </summary>
        public MidiTimeCodeComponent Component { get; set; }

        /// <summary>
        /// Gets or sets value of the MIDI time code component presented by the current <see cref="MidiTimeCodeEvent"/>.
        /// </summary>
        public FourBitNumber ComponentValue { get; set; }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            var data = reader.ReadByte();

            var midiTimeCodeComponent = (byte)data.GetHead();
            if (!Enum.IsDefined(typeof(MidiTimeCodeComponent), midiTimeCodeComponent))
                throw new InvalidMidiTimeCodeComponentException(midiTimeCodeComponent);

            Component = (MidiTimeCodeComponent)midiTimeCodeComponent;

            var componentValue = data.GetTail();
            if (componentValue > ComponentValueMasks[Component])
            {
                switch (settings.InvalidSystemCommonEventParameterValuePolicy)
                {
                    case InvalidSystemCommonEventParameterValuePolicy.Abort:
                        throw new InvalidSystemCommonEventParameterValueException(EventType, $"{nameof(ComponentValue)} (component is {Component})", componentValue);
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

            var data = DataTypesUtilities.CombineAsFourBitNumbers((byte)component, (byte)componentValue);

            writer.WriteByte(data);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new MidiTimeCodeEvent(Component, ComponentValue);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"MIDI Time Code ({Component}, {ComponentValue})";
        }

        #endregion
    }
}

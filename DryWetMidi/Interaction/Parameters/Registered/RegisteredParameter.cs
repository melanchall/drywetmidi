using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents MIDI registered parameter (RPN).
    /// </summary>
    public abstract class RegisteredParameter : Parameter
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredParameter"/> with the specified
        /// type of a parameter.
        /// </summary>
        /// <param name="parameterType">The type of parameter.</param>
        protected RegisteredParameter(RegisteredParameterType parameterType)
        {
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the current parameter.
        /// </summary>
        public RegisteredParameterType ParameterType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns MSB and LSB that represent data of the current parameter.
        /// </summary>
        /// <param name="msb">MSB of parameter's data.</param>
        /// <param name="lsb">LSB of parameter's data.</param>
        protected abstract void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb);

        /// <summary>
        /// Returns the number of increment/decrement steps based on the value of the
        /// current parameter.
        /// </summary>
        /// <returns>The number of increment/decrement steps based on the value of the
        /// current parameter.</returns>
        protected abstract int GetIncrementStepsCount();

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the collection of <see cref="TimedEvent"/> objects that represent the current
        /// parameter. In fact, each <see cref="TimedEvent"/> object will contain <see cref="ControlChangeEvent"/> event.
        /// </summary>
        /// <returns>Collection of <see cref="TimedEvent"/> objects that represent the current
        /// parameter.</returns>
        public override IEnumerable<TimedEvent> GetTimedEvents()
        {
            var controlChanges = new List<(ControlName ControlName, SevenBitNumber ControlValue)>
            {
                (ControlName.RegisteredParameterNumberMsb, RegisteredParameterNumbers.GetMsb(ParameterType)),
                (ControlName.RegisteredParameterNumberLsb, RegisteredParameterNumbers.GetLsb(ParameterType))
            };

            switch (ValueType)
            {
                case ParameterValueType.Exact:
                    {
                        GetData(out var dataMsb, out var dataLsb);
                        
                        controlChanges.Add((ControlName.DataEntryMsb, dataMsb));
                        if (dataLsb != null)
                            controlChanges.Add((ControlName.LsbForDataEntry, dataLsb.Value));

                        break;
                    }
                case ParameterValueType.Increment:
                case ParameterValueType.Decrement:
                    {
                        var controlName = ValueType == ParameterValueType.Increment
                            ? ControlName.DataIncrement
                            : ControlName.DataDecrement;

                        controlChanges.AddRange(Enumerable
                            .Range(0, GetIncrementStepsCount())
                            .Select(i => (controlName, SevenBitNumber.MaxValue)));

                        break;
                    }
            }

            controlChanges.Add((ControlName.RegisteredParameterNumberMsb, (SevenBitNumber)0x7F));
            controlChanges.Add((ControlName.RegisteredParameterNumberLsb, (SevenBitNumber)0x7F));

            return controlChanges.Select(controlChange => new TimedEvent(
                controlChange.ControlName.GetControlChangeEvent(controlChange.ControlValue, Channel),
                Time));
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"RPN {ParameterType} set to {ValueType}";
        }

        #endregion
    }
}

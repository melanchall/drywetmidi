using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public abstract class RegisteredParameter : Parameter
    {
        #region Constructor

        protected RegisteredParameter(RegisteredParameterType parameterType)
        {
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        // TODO: test that each value used for single type
        public RegisteredParameterType ParameterType { get; }

        #endregion

        #region Methods

        protected abstract void GetData(out SevenBitNumber msb, out SevenBitNumber? lsb);

        protected abstract int GetIncrementStepsCount();

        #endregion

        #region Overrides

        public override IEnumerable<TimedEvent> GetTimedEvents()
        {
            var controlChanges = new List<Tuple<ControlName, SevenBitNumber>>
            {
                Tuple.Create(ControlName.RegisteredParameterNumberMsb, RegisteredParameterNumbers.GetMsb(ParameterType)),
                Tuple.Create(ControlName.RegisteredParameterNumberLsb, RegisteredParameterNumbers.GetLsb(ParameterType))
            };

            switch (ValueType)
            {
                case ParameterValueType.Exact:
                    {
                        SevenBitNumber dataMsb;
                        SevenBitNumber? dataLsb;
                        GetData(out dataMsb, out dataLsb);
                        
                        controlChanges.Add(Tuple.Create(ControlName.DataEntryMsb, dataMsb));
                        if (dataLsb != null)
                            controlChanges.Add(Tuple.Create(ControlName.LsbForDataEntry, dataLsb.Value));

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
                            .Select(i => Tuple.Create(controlName, SevenBitNumber.MaxValue)));

                        break;
                    }
            }

            controlChanges.Add(Tuple.Create(ControlName.RegisteredParameterNumberMsb, (SevenBitNumber)0x7F));
            controlChanges.Add(Tuple.Create(ControlName.RegisteredParameterNumberLsb, (SevenBitNumber)0x7F));

            return controlChanges.Select(controlChange => new TimedEvent(
                controlChange.Item1.GetControlChangeEvent(controlChange.Item2, Channel),
                Time));
        }

        public override string ToString()
        {
            return $"RPN {ParameterType} set to {ValueType}";
        }

        #endregion
    }
}

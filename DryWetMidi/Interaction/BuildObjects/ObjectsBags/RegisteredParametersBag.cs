using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RegisteredParametersBag : ObjectsBag
    {
        #region Fields

        private ControlChangeEvent _startMsbEvent;
        private ControlChangeEvent _startLsbEvent;
        private ControlChangeEvent _dataMsbEvent;
        private ControlChangeEvent _dataLsbEvent;
        private ControlChangeEvent _incrementEvent;
        private ControlChangeEvent _decrementEvent;
        private ControlChangeEvent _endMsbEvent;
        private ControlChangeEvent _endLsbEvent;

        private bool _canObjectsBeAdded = true;

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanObjectsBeAdded => _canObjectsBeAdded;

        #endregion

        #region Methods

        public override IEnumerable<ITimedObject> GetRawObjects()
        {
            throw new NotImplementedException();
        }

        public override bool TryAddObject(ITimedObject timedObject, IBuildingContext context, ObjectsBuildingSettings settings)
        {
            if (!CanObjectsBeAdded)
                return false;

            var buildingContext = (RegisteredParametersContext)context;

            return TryAddTimedEvent(timedObject as TimedEvent, buildingContext);
        }

        private bool TryAddTimedEvent(TimedEvent timedEvent, RegisteredParametersContext context)
        {
            if (timedEvent == null)
                return false;

            var controlChangeEvent = timedEvent.Event as ControlChangeEvent;
            if (controlChangeEvent == null)
                return false;

            var controlName = controlChangeEvent.GetControlName();
            switch (controlName)
            {
                case ControlName.RegisteredParameterNumberMsb:
                    break;
                case ControlName.RegisteredParameterNumberLsb:
                    break;
                case ControlName.DataEntryMsb:
                    break;
                case ControlName.LsbForDataEntry:
                    break;
                case ControlName.DataIncrement:
                    break;
                case ControlName.DataDecrement:
                    break;
            }
            
            throw new NotImplementedException();

            return true;
        }

        #endregion
    }
}

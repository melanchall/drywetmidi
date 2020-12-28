using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal interface IOverlayBuilder
    {
        #region Methods

        IEnumerable<ITimedObject> BuildObjects(
            IEnumerable<ITimedObject> inputTimedObjects,
            IEnumerable<ITimedObject> resultTimedObjects,
            ObjectType objectType,
            ObjectsBuildingSettings settings);

        #endregion
    }
}

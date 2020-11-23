using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class ObjectsBag
    {
        #region Fields

        protected readonly List<ITimedObject> _timedObjects = new List<ITimedObject>();

        #endregion

        #region Properties

        public abstract bool IsCompleted { get; }

        public abstract bool CanObjectsBeAdded { get; }

        #endregion

        #region Methods

        public virtual IEnumerable<ITimedObject> GetObjects()
        {
            return _timedObjects;
        }

        public abstract IEnumerable<ITimedObject> GetRawObjects();

        public abstract bool TryAddObject(ITimedObject timedObject, IBuildingContext context, ObjectsBuildingSettings settings);

        #endregion
    }
}

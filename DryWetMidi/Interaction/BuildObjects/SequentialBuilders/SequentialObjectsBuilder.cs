using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal abstract class SequentialObjectsBuilder<TBag, TContext> : ISequentialObjectsBuilder
        where TBag : ObjectsBag, new()
        where TContext : IBuildingContext, new()
    {
        #region Fields

        private readonly ObjectsBuildingSettings _settings;

        private readonly List<ObjectsBag> _objectsBags;
        private readonly List<TBag> _uncompletedBags = new List<TBag>();

        private readonly TContext _context;

        #endregion

        #region Constructors

        public SequentialObjectsBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
        {
            _objectsBags = objectsBags;
            _settings = settings;

            _context = new TContext();
        }

        #endregion

        #region Methods

        public bool TryAddObject(ITimedObject timedObject)
        {
            var handlingBag = _uncompletedBags.FirstOrDefault(b => b.TryAddObject(timedObject, _context, _settings));
            if (handlingBag != null)
            {
                if (!handlingBag.CanObjectsBeAdded)
                    _uncompletedBags.Remove(handlingBag);

                return true;
            }

            //

            var bag = new TBag();
            var result = bag.TryAddObject(timedObject, _context, _settings);
            if (result)
            {
                _objectsBags.Add(bag);

                if (bag.CanObjectsBeAdded)
                    _uncompletedBags.Add(bag);
            }

            //

            return result;
        }

        #endregion
    }
}

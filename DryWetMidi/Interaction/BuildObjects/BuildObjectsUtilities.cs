using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public static class BuildObjectsUtilities
    {
        #region Methods

        public static IEnumerable<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectsBuildingSettings settings)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            timedObjects = timedObjects.OrderBy(o => o.Time);

            var result = BuildObjects(timedObjects, settings, 0);
            result = AddObjectsByPostBuilders(timedObjects, result, settings);

            return result;
        }

        private static IEnumerable<ITimedObject> AddObjectsByPostBuilders(
            IEnumerable<ITimedObject> inputTimedObjects,
            IEnumerable<ITimedObject> resultTimedObjects,
            ObjectsBuildingSettings settings)
        {
            var builders = new IOverlayBuilder[]
            {
                settings.BuildRests ? new RestsBuilder() : null
            }
            .Where(b => b != null)
            .ToArray();

            if (builders.Any())
            {
                var resultList = resultTimedObjects.ToList();

                foreach (var builder in builders)
                {
                    resultList.AddRange(builder.BuildObjects(inputTimedObjects, resultTimedObjects, settings));
                }

                resultTimedObjects = resultList.OrderBy(o => o.Time);
            }

            return resultTimedObjects;
        }

        private static IEnumerable<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectsBuildingSettings settings,
            int buildersStartIndex)
        {
            var objectsBags = new List<ObjectsBag>();

            var builders = new ISequentialObjectsBuilder[]
            {
                settings.BuildChords ? new ChordsBuilder(objectsBags, settings) : null,
                settings.BuildNotes ? new NotesBuilder(objectsBags, settings) : null,
                settings.BuildRegisteredParameters ? new RegisteredParametersBuilder(objectsBags, settings) : null,
                settings.BuildTimedEvents ? new TimedEventsBuilder(objectsBags, settings) : null,
            }
            .Where(b => b != null)
            .Skip(buildersStartIndex)
            .ToArray();

            //

            foreach (var timedObject in timedObjects)
            {
                if (timedObject == null)
                {
                    // TODO: policy
                    continue;
                }

                var handlingBuilder = builders.FirstOrDefault(m => m.TryAddObject(timedObject));
                if (handlingBuilder == null)
                {
                    // TODO: policy
                    continue;
                }
            }

            //

            return objectsBags
                .SelectMany(b => b.IsCompleted
                    ? b.GetObjects()
                    : BuildObjects(b.GetRawObjects(), settings, buildersStartIndex + 1))
                .OrderBy(o => o.Time);
        }

        #endregion
    }
}

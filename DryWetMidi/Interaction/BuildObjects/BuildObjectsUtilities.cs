using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public static class BuildObjectsUtilities
    {
        #region Methods

        public static ICollection<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            return timedObjects.BuildObjects(objectType, settings, true);
        }

        public static ICollection<ITimedObject> BuildObjects(
            this IEnumerable<MidiEvent> midiEvents,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return midiEvents.GetTimedEventsLazy().BuildObjects(objectType, settings, false);
        }

        public static ICollection<ITimedObject> BuildObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return ((IEnumerable<MidiEvent>)eventsCollection).BuildObjects(objectType, settings);
        }

        public static ICollection<ITimedObject> BuildObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.BuildObjects(objectType, settings);
        }

        public static ICollection<ITimedObject> BuildObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            return eventsCollections.GetTimedEventsLazy(eventsCount).Select(e => e.Item1).BuildObjects(objectType, settings, false);
        }

        public static ICollection<ITimedObject> BuildObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectsBuildingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().BuildObjects(objectType, settings);
        }

        private static ICollection<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectsBuildingSettings settings,
            bool sortObjects)
        {
            if (settings == null)
                settings = new ObjectsBuildingSettings();

            timedObjects = timedObjects.Where(o => o != null);
            if (sortObjects)
                timedObjects = timedObjects.OrderBy(o => o.Time);

            var result = BuildObjects(timedObjects, objectType, settings, 0);
            result = AddObjectsByPostBuilders(timedObjects, result, objectType, settings);

            return result;
        }

        private static ICollection<ITimedObject> AddObjectsByPostBuilders(
            IEnumerable<ITimedObject> inputTimedObjects,
            ICollection<ITimedObject> resultTimedObjects,
            ObjectType objectType,
            ObjectsBuildingSettings settings)
        {
            var builders = new IOverlayBuilder[]
            {
                objectType.HasFlag(ObjectType.Rest) ? new RestsBuilder() : null
            }
            .Where(b => b != null)
            .ToArray();

            if (builders.Any())
            {
                var resultList = resultTimedObjects.ToList();

                foreach (var builder in builders)
                {
                    resultList.AddRange(builder.BuildObjects(inputTimedObjects, resultTimedObjects, objectType, settings));
                }

                resultTimedObjects = resultList.OrderBy(o => o.Time).ToArray();
            }

            return resultTimedObjects;
        }

        private static ICollection<ITimedObject> BuildObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectsBuildingSettings settings,
            int buildersStartIndex)
        {
            var objectsBags = new List<ObjectsBag>();

            var builders = new ISequentialObjectsBuilder[]
            {
                objectType.HasFlag(ObjectType.Chord) ? new ChordsBuilder(objectsBags, settings) : null,
                objectType.HasFlag(ObjectType.Note) ? new NotesBuilder(objectsBags, settings) : null,
                objectType.HasFlag(ObjectType.RegisteredParameter) ? new RegisteredParametersBuilder(objectsBags, settings) : null,
                objectType.HasFlag(ObjectType.TimedEvent) ? new TimedEventsBuilder(objectsBags, settings) : null,
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
                    : BuildObjects(b.GetRawObjects(), objectType, settings, buildersStartIndex + 1))
                .OrderBy(o => o.Time)
                .ToArray();
        }

        #endregion
    }
}

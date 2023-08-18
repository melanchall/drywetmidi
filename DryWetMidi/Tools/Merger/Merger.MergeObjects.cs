using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to merge nearby objects. More info in the <see href="xref:a_merger">Merger</see> article.
    /// </summary>
    public static partial class Merger
    {
        #region Nested classes

        private interface IObjectDescriptor
        {
            bool IsCompleted { get; set; }

            ITimedObject GetObject(ObjectsMergingSettings settings);
        }

        private sealed class ObjectDescriptor : IObjectDescriptor
        {
            private readonly ITimedObject _timedObject;

            public ObjectDescriptor(ITimedObject timedObject)
            {
                _timedObject = timedObject;
            }

            public bool IsCompleted { get; set; } = true;

            public ITimedObject GetObject(ObjectsMergingSettings settings)
            {
                return _timedObject;
            }
        }

        private sealed class ObjectsMergerDescriptor : IObjectDescriptor
        {
            public ObjectsMergerDescriptor(ObjectsMerger objectsMerger)
            {
                ObjectsMerger = objectsMerger;
            }

            public bool IsCompleted { get; set; }

            public ObjectsMerger ObjectsMerger { get; }

            public ITimedObject GetObject(ObjectsMergingSettings settings)
            {
                return ObjectsMerger.MergeObjects(settings);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Merges nearby objects within the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to merge objects within.</param>
        /// <param name="objectType">Combination of desired types of objects to merge.</param>
        /// <param name="tempoMap">Tempo map used to calculate distances between objects.</param>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void MergeObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            TempoMap tempoMap,
            ObjectsMergingSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var objectsManager = new TimedObjectsManager(trackChunk.Events, objectType, objectDetectionSettings))
            {
                var objects = objectsManager.Objects;
                var newObjects = MergeObjects(objects, tempoMap, false, settings).ToList();

                objects.Clear();
                objects.Add(newObjects);
            }
        }

        /// <summary>
        /// Merges nearby objects within the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to merge objects within.</param>
        /// <param name="objectType">Combination of desired types of objects to merge.</param>
        /// <param name="tempoMap">Tempo map used to calculate distances between objects.</param>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void MergeObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            TempoMap tempoMap,
            ObjectsMergingSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.MergeObjects(objectType, tempoMap, settings, objectDetectionSettings);
            }
        }

        /// <summary>
        /// Merges nearby objects within the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to merge objects within.</param>
        /// <param name="objectType">Combination of desired types of objects to merge.</param>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static void MergeObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectsMergingSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().MergeObjects(objectType, tempoMap, settings, objectDetectionSettings);
        }

        /// <summary>
        /// Merges nearby objects.
        /// </summary>
        /// <param name="objects">Objects that should be merged.</param>
        /// <param name="tempoMap">Tempo map used to calculate distances between objects.</param>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <returns>Collection of new objects that are result of merging of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<ITimedObject> MergeObjects(
            this IEnumerable<ITimedObject> objects,
            TempoMap tempoMap,
            ObjectsMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return MergeObjects(objects, tempoMap, true, settings);
        }

        private static IEnumerable<ITimedObject> MergeObjects(
            IEnumerable<ITimedObject> objects,
            TempoMap tempoMap,
            bool prepareForEnumeration,
            ObjectsMergingSettings settings)
        {
            settings = settings ?? new ObjectsMergingSettings();

            var objectsDescriptors = new LinkedList<IObjectDescriptor>();
            var objectsMergersNodes = new Dictionary<object, LinkedListNode<IObjectDescriptor>>();

            var objectsMergerFactory = settings.ObjectsMergerFactory ?? (obj => new ObjectsMerger(obj));

            var preparedObjects = objects;
            if (prepareForEnumeration)
                preparedObjects = objects.Where(n => n != null).OrderBy(n => n.Time);

            foreach (var obj in preparedObjects)
            {
                var lengthedObject = obj as ILengthedObject;
                if (lengthedObject == null || settings.Filter?.Invoke(obj) == false)
                {
                    if (objectsDescriptors.Count == 0)
                        yield return obj;
                    else
                        objectsDescriptors.AddLast(new ObjectDescriptor(obj));

                    continue;
                }

                var objectId = lengthedObject.GetObjectId();

                LinkedListNode<IObjectDescriptor> node;
                if (!objectsMergersNodes.TryGetValue(objectId, out node))
                {
                    CreateObjectsMerger(objectId, lengthedObject, objectsDescriptors, objectsMergersNodes, objectsMergerFactory);
                    continue;
                }

                var objectsMerger = ((ObjectsMergerDescriptor)node.Value).ObjectsMerger;
                if (objectsMerger.CanAddObject(lengthedObject, tempoMap, settings))
                {
                    objectsMerger.AddObject(lengthedObject);
                }
                else
                {
                    node.Value.IsCompleted = true;

                    var previousNode = node.Previous;
                    if (previousNode == null)
                    {
                        for (var n = node; n != null;)
                        {
                            if (!n.Value.IsCompleted)
                                break;

                            yield return n.Value.GetObject(settings);

                            var next = n.Next;
                            objectsDescriptors.Remove(n);
                            n = next;
                        }
                    }

                    CreateObjectsMerger(objectId, lengthedObject, objectsDescriptors, objectsMergersNodes, objectsMergerFactory);
                }
            }

            foreach (var objectDescriptor in objectsDescriptors)
            {
                yield return objectDescriptor.GetObject(settings);
            }
        }

        private static void CreateObjectsMerger(
            object objectId,
            ILengthedObject lengthedObject,
            LinkedList<IObjectDescriptor> objectsDescriptors,
            Dictionary<object, LinkedListNode<IObjectDescriptor>> objectsMergers,
            Func<ILengthedObject, ObjectsMerger> objectsMergerFactory)
        {
            objectsMergers[objectId] = objectsDescriptors.AddLast(new ObjectsMergerDescriptor(objectsMergerFactory(lengthedObject)));
        }

        #endregion
    }
}

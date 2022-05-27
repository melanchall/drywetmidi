using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Merger
    {
        #region Nested classes

        private interface IObjectId
        {
        }

        private sealed class NoteId : IObjectId
        {
            public NoteId(FourBitNumber channel, SevenBitNumber noteNumber)
            {
                Channel = channel;
                NoteNumber = noteNumber;
            }

            public FourBitNumber Channel { get; }

            public SevenBitNumber NoteNumber { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, this))
                    return true;

                var noteId = obj as NoteId;
                if (ReferenceEquals(noteId, null))
                    return false;

                return Channel == noteId.Channel &&
                       NoteNumber == noteId.NoteNumber;
            }

            public override int GetHashCode()
            {
                return Channel * 1000 + NoteNumber;
            }
        }

        private sealed class RestId : IObjectId
        {
            public RestId(FourBitNumber? channel, SevenBitNumber? noteNumber)
            {
                Channel = channel;
                NoteNumber = noteNumber;
            }

            public FourBitNumber? Channel { get; }

            public SevenBitNumber? NoteNumber { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, this))
                    return true;

                var restId = obj as RestId;
                if (ReferenceEquals(restId, null))
                    return false;

                return Channel == restId.Channel &&
                       NoteNumber == restId.NoteNumber;
            }

            public override int GetHashCode()
            {
                return (Channel ?? 20) * 1000 + (NoteNumber ?? 200);
            }
        }

        private sealed class ChordId : IObjectId
        {
            public ChordId(ICollection<NoteId> notesIds)
            {
                NotesIds = notesIds;
            }

            public ICollection<NoteId> NotesIds { get; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, this))
                    return true;

                var chordId = obj as ChordId;
                if (ReferenceEquals(chordId, null))
                    return false;

                // TODO: ignore order
                return NotesIds.SequenceEqual(chordId.NotesIds);
            }

            public override int GetHashCode()
            {
                return NotesIds.Sum(id => id.GetHashCode());
            }
        }

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
            private readonly ObjectsMerger _objectsMerger;

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
            var objectsMergersNodes = new Dictionary<IObjectId, LinkedListNode<IObjectDescriptor>>();

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

                var objectId = GetObjectId(lengthedObject);

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
                    if (previousNode != null)
                        break;

                    for (var n = node; n != null;)
                    {
                        if (!n.Value.IsCompleted)
                            break;

                        yield return n.Value.GetObject(settings);

                        var next = n.Next;
                        objectsDescriptors.Remove(n);
                        n = next;
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
            IObjectId objectId,
            ILengthedObject lengthedObject,
            LinkedList<IObjectDescriptor> objectsDescriptors,
            Dictionary<IObjectId, LinkedListNode<IObjectDescriptor>> objectsMergers,
            Func<ILengthedObject, ObjectsMerger> objectsMergerFactory)
        {
            objectsMergers[objectId] = objectsDescriptors.AddLast(new ObjectsMergerDescriptor(objectsMergerFactory(lengthedObject)));
        }

        private static IObjectId GetObjectId(ILengthedObject obj)
        {
            var note = obj as Note;
            if (note != null)
                return GetNoteId(note);

            var chord = obj as Chord;
            if (chord != null)
                return new ChordId(chord.Notes.Select(GetNoteId).ToArray());

            var rest = obj as Rest;
            if (rest != null)
                return new RestId(rest.Channel, rest.NoteNumber);

            throw new NotImplementedException($"Getting of ID for {obj} is not implemented.");
        }

        private static NoteId GetNoteId(Note note)
        {
            return new NoteId(note.Channel, note.NoteNumber);
        }

        #endregion
    }
}

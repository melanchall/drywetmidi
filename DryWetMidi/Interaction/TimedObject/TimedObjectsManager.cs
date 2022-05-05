using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    public class TimedObjectsManager : TimedObjectsManager<ITimedObject>
    {
        #region Constructor

        public TimedObjectsManager(
            EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings = null,
            TimedObjectsComparer comparer = null)
            : base(eventsCollection, objectType, objectDetectionSettings, comparer)
        {
        }

        #endregion
    }

    public class TimedObjectsManager<TObject> : IDisposable
        where TObject : ITimedObject
    {
        #region Fields

        private EventsCollection _eventsCollection;

        private TimedObjectsCollection<ITimedObject> _backgroundObjects;
        private TimedObjectsComparer _comparer;

        private bool _disposed;

        #endregion

        #region Constructor

        public TimedObjectsManager(
            EventsCollection eventsCollection,
            ObjectDetectionSettings objectDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            Initialize(
                eventsCollection,
                GetObjectType(),
                objectDetectionSettings,
                comparer);
        }

        internal TimedObjectsManager(
            EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings = null,
            TimedObjectsComparer comparer = null)
        {
            Initialize(
                eventsCollection,
                objectType,
                objectDetectionSettings,
                comparer);
        }

        #endregion

        #region Properties

        public TimedObjectsCollection<TObject> Objects { get; private set; }

        #endregion

        #region Methods

        public void SaveChanges()
        {
            _eventsCollection.Clear();
            _eventsCollection.AddObjects(
                _backgroundObjects
                    .Concat(Objects.OfType<ITimedObject>())
                    .OrderBy(obj => obj, _comparer));
        }

        private ObjectType GetObjectType()
        {
            var type = typeof(TObject);

            if (type == typeof(Note))
                return ObjectType.Note;
            if (type == typeof(Chord))
                return ObjectType.Chord;
            if (type == typeof(TimedEvent))
                return ObjectType.TimedEvent;

            throw new InvalidOperationException($"Objects of '{type}' type are not supported.");
        }

        private ObjectType GetObjectType(ITimedObject timedObject)
        {
            if (timedObject is Note)
                return ObjectType.Note;
            if (timedObject is Chord)
                return ObjectType.Chord;
            if (timedObject is TimedEvent)
                return ObjectType.TimedEvent;

            throw new InvalidOperationException($"Objects of '{timedObject.GetType()}' type are not supported.");
        }

        private void Initialize(
            EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            TimedObjectsComparer comparer)
        {
            _eventsCollection = eventsCollection;
            _comparer = comparer ?? new TimedObjectsComparer();
            _backgroundObjects = new TimedObjectsCollection<ITimedObject>(Enumerable.Empty<ITimedObject>(), _comparer);

            var allObjects = eventsCollection.GetObjects(objectType | ObjectType.TimedEvent, objectDetectionSettings);
            var objects = new TimedObjectsCollection<TObject>(Enumerable.Empty<TObject>(), _comparer);

            foreach (var obj in allObjects)
            {
                if (objectType.HasFlag(GetObjectType(obj)))
                    objects.Add((TObject)obj);
                else
                    _backgroundObjects.Add(obj);
            }

            Objects = objects;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                SaveChanges();

            _disposed = true;
        }

        #endregion
    }
}

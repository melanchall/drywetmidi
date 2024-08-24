using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage timed objects of different types within an <see cref="EventsCollection"/>
    /// (which can be obtained via <see cref="TrackChunk.Events"/> for example). More info in the
    /// <see href="xref:a_managers">Objects managers</see> article.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To start manage objects you need to get an instance of the <see cref="TimedObjectsManager"/>.
    /// </para>
    /// <para>
    /// To finish managing you need to call the <see cref="TimedObjectsManager{TObject}.SaveChanges"/> or
    /// <see cref="TimedObjectsManager{TObject}.Dispose()"/> method.
    /// </para>
    /// <para>
    /// Since the manager implements <see cref="IDisposable"/> it is recommended to manage objects within
    /// the <c>using</c> block.
    /// </para>
    /// </remarks>
    public sealed class TimedObjectsManager : TimedObjectsManager<ITimedObject>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedObjectsManager"/> with the specified
        /// events collection and object type. Optionally object detection settings and comparer can be provided.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to manage objects within.</param>
        /// <param name="objectType">The type of objects to manage.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating
        /// <see cref="TimedObjectsManager{TObject}.Objects"/> or saving objects back to the <paramref name="eventsCollection"/>
        /// via <see cref="TimedObjectsManager{TObject}.SaveChanges"/> or <see cref="TimedObjectsManager{TObject}.Dispose()"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
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

    /// <summary>
    /// Provides a way to manage timed objects of the specified type within an <see cref="EventsCollection"/>
    /// (which can be obtained via <see cref="TrackChunk.Events"/> for example). More info in the
    /// <see href="xref:a_managers">Objects managers</see> article.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To start manage objects you need to get an instance of the <see cref="TimedObjectsManager{TObject}"/>.
    /// Also <c>ManageX</c> methods within specific object type utilities class (for example,
    /// <see cref="NotesManagingUtilities.ManageNotes(TrackChunk, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)"/>
    /// or <see cref="ChordsManagingUtilities.ManageChords(TrackChunk, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)"/>)
    /// can be used.
    /// </para>
    /// <para>
    /// To finish managing you need to call the <see cref="SaveChanges"/> or <see cref="Dispose()"/> method.
    /// </para>
    /// <para>
    /// Since the manager implements <see cref="IDisposable"/> it is recommended to manage objects within
    /// the <c>using</c> block.
    /// </para>
    /// </remarks>
    /// <seealso cref="TimedEventsManagingUtilities"/>
    /// <seealso cref="NotesManagingUtilities"/>
    /// <seealso cref="ChordsManagingUtilities"/>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedObjectsManager{TObject}"/> with the specified
        /// events collection. Optionally object detection settings and comparer can be provided.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to manage objects within.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="comparer">Comparer that will be used to order objects on enumerating
        /// <see cref="Objects"/> or saving objects back to the <paramref name="eventsCollection"/>
        /// via <see cref="SaveChanges"/> or <see cref="Dispose()"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Gets the <see cref="TimedObjectsCollection{TObject}"/> with all objects managed by the current
        /// <see cref="TimedObjectsManager{TObject}"/>.
        /// </summary>
        public TimedObjectsCollection<TObject> Objects { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Saves all objects that were managed with the current <see cref="TimedObjectsManager{TObject}"/>
        /// updating underlying events collection.
        /// </summary>
        /// <remarks>
        /// This method will rewrite content of the events collection was used to construct the current
        /// <see cref="TimedObjectsManager{TObject}"/> with events were managed by this manager.
        /// </remarks>
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

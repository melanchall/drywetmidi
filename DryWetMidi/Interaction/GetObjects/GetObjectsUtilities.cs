using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides utilities to extract objects of different types at once.
    /// </summary>
    /// <remarks>
    /// Please see <see href="xref:a_getting_objects#getobjects">Getting objects
    /// (section GetObjects)</see> article to learn more.
    /// </remarks>
    public static class GetObjectsUtilities
    {
        #region Constants

        private static readonly object NoSeparationNoteDescriptor = new object();

        private static readonly Dictionary<RestSeparationPolicy, Func<Note, object>> NoteDescriptorProviders =
            new Dictionary<RestSeparationPolicy, Func<Note, object>>
            {
                [RestSeparationPolicy.NoSeparation] = n => NoSeparationNoteDescriptor,
                [RestSeparationPolicy.SeparateByChannel] = n => n.Channel,
                [RestSeparationPolicy.SeparateByNoteNumber] = n => n.NoteNumber,
                [RestSeparationPolicy.SeparateByChannelAndNoteNumber] = n => n.GetNoteId()
            };

        private static readonly Dictionary<RestSeparationPolicy, bool> SetRestChannel =
            new Dictionary<RestSeparationPolicy, bool>
            {
                [RestSeparationPolicy.NoSeparation] = false,
                [RestSeparationPolicy.SeparateByChannel] = true,
                [RestSeparationPolicy.SeparateByNoteNumber] = false,
                [RestSeparationPolicy.SeparateByChannelAndNoteNumber] = true
            };

        private static readonly Dictionary<RestSeparationPolicy, bool> SetRestNoteNumber =
            new Dictionary<RestSeparationPolicy, bool>
            {
                [RestSeparationPolicy.NoSeparation] = false,
                [RestSeparationPolicy.SeparateByChannel] = false,
                [RestSeparationPolicy.SeparateByNoteNumber] = true,
                [RestSeparationPolicy.SeparateByChannelAndNoteNumber] = true
            };

        #endregion

        #region Methods

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="midiEvents">Collection of <see cref="MidiEvent"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="midiEvents"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<MidiEvent> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return midiEvents
                .GetTimedEventsLazy()
                .GetObjectsFromSortedTimedObjects(0, objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="eventsCollection"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection
                .GetTimedEventsLazy()
                .GetObjectsFromSortedTimedObjects(eventsCollection.Count / 2, objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="trackChunk"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetObjects(objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="trackChunks"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var timedEvents = eventsCollections.GetTimedEventsLazy(eventsCount);
            var timedObjects = (IEnumerable<ITimedObject>)timedEvents.Select(o => o.Item1);

            if (objectType.HasFlag(ObjectType.Chord) || objectType.HasFlag(ObjectType.Note) || objectType.HasFlag(ObjectType.Rest))
            {
                timedObjects = !objectType.HasFlag(ObjectType.Chord)
                    ? timedEvents.GetNotesAndTimedEventsLazy(settings?.NoteDetectionSettings ?? new NoteDetectionSettings()).Select(o => o.Item1)
                    : timedEvents.GetChordsAndNotesAndTimedEventsLazy(settings?.ChordDetectionSettings ?? new ChordDetectionSettings()).Select(o => o.Item1);
            }

            return timedObjects.GetObjectsFromSortedTimedObjects(eventsCount / 2, objectType, settings, false);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="midiFile"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().GetObjects(objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="ITimedObject"/>.
        /// </summary>
        /// <param name="timedObjects">Collection of <see cref="ITimedObject"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="timedObjects"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);

            var resultCollectionSize = 0;
            var processedTimedObjects = new List<ITimedObject>();

            foreach (var timedObject in timedObjects)
            {
                var processed =
                    TryProcessTimedEvent(timedObject as TimedEvent, processedTimedObjects) ||
                    TryProcessNote(timedObject as Note, processedTimedObjects, getNotes, getChords) ||
                    TryProcessChord(timedObject as Chord, processedTimedObjects, getNotes, getChords);

                if (processed)
                    resultCollectionSize++;
            }

            return GetObjectsFromSortedTimedObjects(
                processedTimedObjects.OrderBy(o => o.Time),
                resultCollectionSize,
                objectType,
                settings);
        }

        private static bool TryProcessTimedEvent(TimedEvent timedEvent, List<ITimedObject> processedTimedObjects)
        {
            if (timedEvent == null)
                return false;

            processedTimedObjects.Add(timedEvent);
            return true;
        }

        private static bool TryProcessNote(Note note, List<ITimedObject> processedTimedObjects, bool getNotes, bool getChords)
        {
            if (note == null)
                return false;

            if (getNotes || getChords)
                processedTimedObjects.Add(note);
            else
            {
                processedTimedObjects.Add(note.TimedNoteOnEvent);
                processedTimedObjects.Add(note.TimedNoteOffEvent);
            }

            return true;
        }

        private static bool TryProcessChord(Chord chord, List<ITimedObject> processedTimedObjects, bool getNotes, bool getChords)
        {
            if (chord == null)
                return false;

            if (getChords)
                processedTimedObjects.Add(chord);
            else if (getNotes)
                processedTimedObjects.AddRange(chord.Notes);
            else
            {
                foreach (var note in chord.Notes)
                {
                    processedTimedObjects.Add(note.TimedNoteOnEvent);
                    processedTimedObjects.Add(note.TimedNoteOffEvent);
                }
            }

            return true;
        }

        private static ICollection<ITimedObject> GetObjectsFromSortedTimedObjects(
            this IEnumerable<ITimedObject> processedTimedObjects,
            int resultCollectionSize,
            ObjectType objectType,
            ObjectDetectionSettings settings,
            bool createNotes = true)
        {
            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);
            var getRests = objectType.HasFlag(ObjectType.Rest);
            var getTimedEvents = objectType.HasFlag(ObjectType.TimedEvent);

            settings = settings ?? new ObjectDetectionSettings();
            var noteDetectionSettings = settings.NoteDetectionSettings ?? new NoteDetectionSettings();
            var chordDetectionSettings = settings.ChordDetectionSettings ?? new ChordDetectionSettings();
            var restDetectionSettings = settings.RestDetectionSettings ?? new RestDetectionSettings();

            var timedObjects = processedTimedObjects;

            if (createNotes && (getChords || getNotes || getRests))
            {
                var notesAndTimedEvents = processedTimedObjects.GetNotesAndTimedEventsLazy(noteDetectionSettings, true);

                timedObjects = getChords
                    ? notesAndTimedEvents.GetChordsAndNotesAndTimedEventsLazy(chordDetectionSettings, true)
                    : notesAndTimedEvents;
            }

            //

            var result = resultCollectionSize > 0
                ? new List<ITimedObject>(resultCollectionSize)
                : new List<ITimedObject>();

            var notesLastEndTimes = new Dictionary<object, long>();
            var noteDescriptorProvider = NoteDescriptorProviders[restDetectionSettings.RestSeparationPolicy];
            var setRestChannel = SetRestChannel[restDetectionSettings.RestSeparationPolicy];
            var setRestNoteNumber = SetRestNoteNumber[restDetectionSettings.RestSeparationPolicy];

            foreach (var timedObject in timedObjects)
            {
                if (getChords)
                {
                    var chord = timedObject as Chord;
                    if (chord != null)
                        result.Add(chord);
                }

                if (getNotes)
                {
                    var note = timedObject as Note;
                    if (note != null)
                        result.Add(note);
                }

                if (getTimedEvents)
                {
                    var timedEvent = timedObject as TimedEvent;
                    if (timedEvent != null)
                        result.Add(timedEvent);
                }

                if (getRests)
                {
                    var note = timedObject as Note;
                    if (note != null)
                    {
                        var noteDescriptor = noteDescriptorProvider(note);

                        long lastEndTime;
                        notesLastEndTimes.TryGetValue(noteDescriptor, out lastEndTime);

                        if (note.Time > lastEndTime)
                        {
                            var rest = new Rest(
                                lastEndTime,
                                note.Time - lastEndTime,
                                setRestChannel ? (FourBitNumber?)note.Channel : null,
                                setRestNoteNumber ? (SevenBitNumber?)note.NoteNumber : null);
                            if (result.Count > 0)
                            {
                                var i = result.Count - 1;

                                for (; i >= 0; i--)
                                {
                                    if (rest.Time >= result[i].Time)
                                        break;
                                }

                                i++;
                                if (i >= result.Count)
                                    result.Add(rest);
                                else
                                    result.Insert(i, rest);
                            }    
                            else
                                result.Add(rest);
                        }

                        notesLastEndTimes[noteDescriptor] = Math.Max(lastEndTime, note.Time + note.Length);
                    }
                }
            }

            result.TrimExcess();
            return result;
        }

        #endregion
    }
}

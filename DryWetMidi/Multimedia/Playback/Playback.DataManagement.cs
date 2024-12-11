using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    public partial class Playback
    {
        #region Fields

        private readonly HashSet<NotePlaybackEventMetadata> _notesMetadataHashSet = new HashSet<NotePlaybackEventMetadata>();

        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOnEvents = new ConcurrentDictionary<NoteId, TimedEvent>();
        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOffEvents = new ConcurrentDictionary<NoteId, TimedEvent>();

        #endregion

        #region Methods

        private void SubscribeToObjectsCollectionChanged(IEnumerable<ITimedObject> timedObjects)
        {
            var observableTimedObjectsCollection = timedObjects as IObservableTimedObjectsCollection;
            if (observableTimedObjectsCollection != null)
                observableTimedObjectsCollection.CollectionChanged += OnObservableTimedObjectsCollectionChanged;
        }

        private void OnObservableTimedObjectsCollectionChanged(object sender, ObservableTimedObjectsCollectionChangedEventArgs e)
        {
            // TODO: what about tempo map events changes

            lock (_playbackLockObject)
            {
                if (e.AddedObjects != null)
                {
                    foreach (var obj in e.AddedObjects)
                    {
                        AddNewTimedObject(obj);
                    }
                }

                if (e.RemovedObjects != null)
                {
                    foreach (var obj in e.RemovedObjects)
                    {
                        RemoveTimedObject(obj, obj.Time);
                    }
                }

                if (e.ChangedObjects != null)
                {
                    foreach (var changedObject in e.ChangedObjects)
                    {
                        var obj = changedObject.TimedObject;
                        var oldTime = changedObject.OldTime;

                        RemoveTimedObject(obj, oldTime);
                        AddNewTimedObject(obj);
                    }
                }

                // TODO: optimize (see below for add, need to think about remove)
                var lastPlaybackEvent = _playbackEvents.GetMaximumNode().Value;
                _duration = lastPlaybackEvent?.Time ?? TimeSpan.Zero;
                _durationInTicks = lastPlaybackEvent?.RawTime ?? 0;

                if (!_playbackEvents.Any())
                {
                    _playbackEventsPosition = null;
                    _beforeStart = true;
                }

                if (IsRunning)
                {
                    SendTrackedData();
                    StopStartNotes();
                    _clock?.Start();
                }

                var currentTime = _clock.CurrentTime;
                if (!Loop && currentTime >= _duration)
                {
                    _clock.StopInternally();
                    OnFinished();
                    return;
                }
            }
        }

        private void AddNewTimedObject(ITimedObject timedObject)
        {
            AddTimedObject(timedObject, TempoMap, false);
        }

        private void RemoveTimedObject(ITimedObject timedObject, long oldTime)
        {
            var timedEvent = timedObject as TimedEvent;
            var noteEvent = timedEvent?.Event as NoteEvent;
            var isNoteOnEvent = noteEvent is NoteOnEvent;

            var time = TimeConverter.ConvertTo<MetricTimeSpan>(oldTime, TempoMap);
            var playbackEventsNodes = _playbackEvents
                .SearchNodes(time)
                .Where(n =>
                {
                    var objectReference = n.Value.ObjectReference;
                    if (object.ReferenceEquals(objectReference, timedObject))
                        return true;

                    if (noteEvent == null)
                        return false;

                    var note = objectReference as Note;
                    if (note == null)
                        return false;

                    return isNoteOnEvent
                        ? object.ReferenceEquals(note.TimedNoteOnEvent, timedEvent)
                        : object.ReferenceEquals(note.TimedNoteOffEvent, timedEvent);
                })
                .SelectMany(n => n.Value.EventsGroup ?? Enumerable.Empty<RedBlackTreeNode<TimeSpan, PlaybackEvent>>())
                .Distinct()
                .ToArray();

            if (noteEvent != null)
            {
                var noteId = new NoteId(noteEvent.Channel, noteEvent.NoteNumber);

                foreach (var playbackEventNode in playbackEventsNodes)
                {
                    var note = (Note)playbackEventNode.Value.ObjectReference;
                    if (isNoteOnEvent)
                        _noteOffEvents[noteId] = note.TimedNoteOffEvent;
                    else
                        _noteOnEvents[noteId] = note.TimedNoteOnEvent;
                }

                if (isNoteOnEvent)
                    _noteOnEvents.TryRemove(noteId, out timedEvent);
                else
                    _noteOffEvents.TryRemove(noteId, out timedEvent);
            }

            foreach (var playbackEventNode in playbackEventsNodes)
            {
                var playbackEvent = playbackEventNode.Value;

                if (playbackEventNode == _playbackEventsPosition)
                {
                    _playbackEventsPosition = _playbackEvents.GetNextNode(_playbackEventsPosition);
                    if (_playbackEventsPosition == null)
                        _beforeStart = false;
                }

                _playbackEvents.Delete(playbackEventNode);

                var noteMetadata = playbackEvent.Metadata.Note;
                if (noteMetadata != null)
                {
                    _notesMetadataHashSet.Remove(noteMetadata);

                    var metadataNodes = _notesMetadata.SearchNodes(noteMetadata.StartTime);

                    foreach (var metadataNode in metadataNodes)
                    {
                        if (metadataNode.Value == noteMetadata)
                            _notesMetadata.Delete(metadataNode);
                    }
                }

                _playbackDataTracker.RemoveData(playbackEvent.Event, oldTime);
            }
        }

        private void InitializePlaybackEvents(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
        {
            foreach (var timedObject in timedObjects)
            {
                AddTimedObject(timedObject, tempoMap, true);
            }
        }

        private void AddTimedObject(ITimedObject timedObject, TempoMap tempoMap, bool isInitialObject)
        {
            if (TryAddNoteEvent(timedObject, tempoMap, isInitialObject))
                return;

            var playbackEvents = GetPlaybackEvents(timedObject, tempoMap);
            var eventsGroup = new HashSet<RedBlackTreeNode<TimeSpan, PlaybackEvent>>();

            var minTime = TimeSpan.MaxValue;

            foreach (var e in playbackEvents)
            {
                var node = _playbackEvents.Add(e.Time, e);
                e.EventsGroup = eventsGroup;

                eventsGroup.Add(node);

                var noteMetadata = e.Metadata.Note;
                if (noteMetadata != null && _notesMetadataHashSet.Add(noteMetadata))
                    _notesMetadata.Add(noteMetadata.StartTime, noteMetadata);

                _playbackDataTracker.InitializeData(
                    e.Event,
                    e.RawTime,
                    e.Metadata.TimedEvent.Metadata);

                if (!isInitialObject && _hasBeenStarted && e.Time > _clock.CurrentTime && (_playbackEventsPosition == null || e.Time < _playbackEventsPosition.Key) && e.Time < minTime)
                {
                    _playbackEventsPosition = node;
                    minTime = e.Time;
                }

                // TODO: see above
                //if (e.Time > _duration)
                //{
                //    _duration = e.Time;
                //    _durationInTicks = e.RawTime;
                //}
            }
        }

        private NoteId GetNoteId(NoteEvent noteEvent)
        {
            return new NoteId(noteEvent.Channel, noteEvent.NoteNumber);
        }

        private bool TryAddNoteEvent(ITimedObject timedObject, TempoMap tempoMap, bool isInitialObject)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    var noteId = GetNoteId(noteEvent);
                    TimedEvent matchedTimedEvent = null;

                    if (noteEvent is NoteOnEvent)
                    {
                        if (_noteOffEvents.TryGetValue(noteId, out matchedTimedEvent) &&
                            GetNoteId((NoteEvent)matchedTimedEvent.Event).Equals(noteId))
                        {
                            _noteOffEvents.TryRemove(noteId, out matchedTimedEvent);
                            AddTimedObject(new Note(timedEvent, matchedTimedEvent, false), TempoMap, isInitialObject);
                        }
                        else
                            _noteOnEvents[noteId] = timedEvent;
                    }
                    else
                    {
                        if (_noteOnEvents.TryGetValue(noteId, out matchedTimedEvent) &&
                            GetNoteId((NoteEvent)matchedTimedEvent.Event).Equals(noteId))
                        {
                            _noteOnEvents.TryRemove(noteId, out matchedTimedEvent);
                            AddTimedObject(new Note(matchedTimedEvent, timedEvent, false), TempoMap, isInitialObject);
                        }
                        else
                            _noteOffEvents[noteId] = timedEvent;
                    }

                    return true;
                }
            }

            return false;
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(ITimedObject timedObject, TempoMap tempoMap)
        {
            var result = new List<PlaybackEvent>();

            var customObjectProcessed = false;
            foreach (var e in GetTimedEvents(timedObject))
            {
                result.Add(GetPlaybackEvent(e, tempoMap));
                customObjectProcessed = true;
            }

            if (customObjectProcessed)
                return result;

            var chord = timedObject as Chord;
            if (chord != null)
            {
                return GetPlaybackEvents(chord, tempoMap);
            }

            var note = timedObject as Note;
            if (note != null)
            {
                return GetPlaybackEvents(note, tempoMap, note);
            }

            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                result.Add(GetPlaybackEvent(timedEvent, tempoMap));
                return result;
            }

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
            {
                result.AddRange(registeredParameter.GetTimedEvents().Select(e => GetPlaybackEvent(e, tempoMap)));
                return result;
            }

            return result;
        }

        private PlaybackEvent GetPlaybackEvent(TimedEvent timedEvent, TempoMap tempoMap)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap, timedEvent);
            playbackEvent.Metadata.TimedEvent = new TimedEventPlaybackEventMetadata((timedEvent as IMetadata)?.Metadata);
            return playbackEvent;
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(Chord chord, TempoMap tempoMap)
        {
            foreach (var note in chord.Notes)
            {
                foreach (var playbackEvent in GetPlaybackEvents(note, tempoMap, chord))
                {
                    yield return playbackEvent;
                }
            }
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(Note note, TempoMap tempoMap, ITimedObject objectReference)
        {
            TimeSpan noteStartTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            TimeSpan noteEndTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap);
            var noteMetadata = new NotePlaybackEventMetadata(note, noteStartTime, noteEndTime);

            yield return GetPlaybackEventWithNoteMetadata(note.GetTimedNoteOnEvent(), tempoMap, noteMetadata, objectReference);
            yield return GetPlaybackEventWithNoteMetadata(note.GetTimedNoteOffEvent(), tempoMap, noteMetadata, objectReference);
        }

        private PlaybackEvent GetPlaybackEventWithNoteMetadata(
            TimedEvent timedEvent,
            TempoMap tempoMap,
            NotePlaybackEventMetadata noteMetadata,
            ITimedObject objectReference)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap, objectReference);
            playbackEvent.Metadata.Note = noteMetadata;
            playbackEvent.Metadata.TimedEvent = new TimedEventPlaybackEventMetadata((timedEvent as IMetadata)?.Metadata);
            return playbackEvent;
        }

        private PlaybackEvent CreatePlaybackEvent(TimedEvent timedEvent, TempoMap tempoMap, ITimedObject objectReference)
        {
            return new PlaybackEvent(
                timedEvent.Event,
                timedEvent.TimeAs<MetricTimeSpan>(tempoMap),
                timedEvent.Time,
                objectReference);
        }

        #endregion
    }
}

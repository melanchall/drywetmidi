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

        private IObservableTimedObjectsCollection _observableTimedObjectsCollection;

        private readonly HashSet<NotePlaybackEventMetadata> _notesMetadataHashSet = new HashSet<NotePlaybackEventMetadata>();

        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOnEvents = new ConcurrentDictionary<NoteId, TimedEvent>();
        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOffEvents = new ConcurrentDictionary<NoteId, TimedEvent>();

        private readonly List<PlaybackEvent> _playbackEventsBuffer = new List<PlaybackEvent>();
        private IPlaybackSource _playbackSource = new FixedPlaybackSource();
        private readonly object _playbackLockObject = new object();
        private bool _beforeStart = true;

        #endregion

        #region Methods

        private void InitializeData(
            IEnumerable<ITimedObject> timedObjects,
            TempoMap tempoMap,
            NoteDetectionSettings noteDetectionSettings,
            bool calculateTempoMap,
            bool useNoteEventsDirectly)
        {
            _observableTimedObjectsCollection = timedObjects as IObservableTimedObjectsCollection;
            if (_observableTimedObjectsCollection != null)
            {
                _playbackSource = new ObservablePlaybackSource();
                _observableTimedObjectsCollection.CollectionChanged += OnObservableTimedObjectsCollectionChanged;
            }

            var playbackEventsSource = timedObjects;
            if (!(timedObjects is ISortedCollection))
                playbackEventsSource = timedObjects.OrderBy(t => t.Time);

            InitializePlaybackEvents(
                playbackEventsSource.GetNotesAndTimedEventsLazy(noteDetectionSettings, true),
                tempoMap,
                calculateTempoMap,
                useNoteEventsDirectly);

            MoveToNextPlaybackEvent();
        }

        private void OnObservableTimedObjectsCollectionChanged(object sender, ObservableTimedObjectsCollectionChangedEventArgs e)
        {
            lock (_playbackLockObject)
            {
                var isRunning = IsRunning;

                if (e.AddedObjects != null)
                {
                    foreach (var obj in e.AddedObjects)
                    {
                        AddNewTimedObject(obj, false);
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
                        var obj = changedObject.Object;
                        var oldTime = changedObject.OldTime;

                        RemoveTimedObject(obj, oldTime);
                        AddNewTimedObject(obj, false);
                    }
                }

                UpdateDuration();
                UpdatePlaybackEndMetric();
                UpdatePlaybackStartMetric();

                if (_playbackSource.IsEmpty())
                {
                    _playbackSource.InvalidatePosition();
                    _beforeStart = true;
                }

                if (isRunning)
                {
                    SendTrackedData();
                    StopStartNotes();
                    _clock?.Start();
                }

                var currentTime = _clock.CurrentTime;
                if (!Loop && currentTime >= _playbackEndMetric)
                {
                    _clock.StopInternally();
                    OnFinished();
                    return;
                }
            }
        }

        private void AddNewTimedObject(ITimedObject timedObject, bool useNoteEventsDirectly)
        {
            AddTimedObject(timedObject, TempoMap, false, true, useNoteEventsDirectly);
        }

        private void RemoveTimedObject(ITimedObject timedObject, long oldTime)
        {
            var playbackSource = (ObservablePlaybackSource)_playbackSource;

            var timedEvent = timedObject as TimedEvent;
            var noteEvent = timedEvent?.Event as NoteEvent;
            var isNoteOnEvent = noteEvent is NoteOnEvent;

            var time = TimeConverter.ConvertTo<MetricTimeSpan>(oldTime, TempoMap);
            var playbackEventsNodes = playbackSource
                .PlaybackEvents
                .GetCoordinatesByKey(time)
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
                .SelectMany(n => n.Value.EventsGroup ?? Enumerable.Empty<RedBlackTreeCoordinate<TimeSpan, PlaybackEvent>>())
                .Distinct()
                .ToArray();

            if (noteEvent != null)
            {
                var noteId = noteEvent.GetNoteId();

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

                if (playbackEventNode.Equals(playbackSource.PlaybackEventsPosition))
                {
                    playbackSource.PlaybackEventsPosition = playbackSource.PlaybackEvents.GetNextCoordinate(playbackSource.PlaybackEventsPosition);
                    if (playbackSource.PlaybackEventsPosition == null)
                        _beforeStart = false;
                }

                playbackSource.PlaybackEvents.Remove(playbackEventNode);

                var noteMetadata = playbackEvent.NoteMetadata;
                if (noteMetadata != null)
                {
                    _notesMetadataHashSet.Remove(noteMetadata);

                    var metadataNodes = _notesMetadata.GetCoordinatesByKey(noteMetadata.StartTime);

                    foreach (var metadataNode in metadataNodes)
                    {
                        if (metadataNode.Value == noteMetadata)
                            _notesMetadata.Remove(metadataNode);
                    }
                }

                RemoveTrackedData(playbackEvent.Event, oldTime);
            }

            TryRemoveSetTempoEvent(
                timedEvent,
                oldTime);

            TryRemoveTimeSignatureEvent(
                timedEvent,
                oldTime);
        }

        private void InitializePlaybackEvents(
            IEnumerable<ITimedObject> timedObjects,
            TempoMap tempoMap,
            bool calculateTempoMap,
            bool useNoteEventsDirectly)
        {
            foreach (var timedObject in timedObjects)
            {
                AddTimedObject(timedObject, tempoMap, true, calculateTempoMap, useNoteEventsDirectly);
            }

            _playbackSource.CompleteSource();
            _notesMetadata.InitializeMax();
        }

        private void AddTimedObject(
            ITimedObject timedObject,
            TempoMap tempoMap,
            bool isInitialObject,
            bool calculateTempoMap,
            bool useNoteEventsDirectly)
        {
            if (TryAddNoteEvent(timedObject, tempoMap, isInitialObject, useNoteEventsDirectly))
                return;

            //

            TimeSpan? nextTempoTime = null;
            bool eventShouldBeAdded = true;
            Tempo oldTempo = null;
            TimeSignature oldTimeSignature = null;

            if (!isInitialObject || calculateTempoMap)
            {
                PrepareSetTempoEventAdding(
                    timedObject as TimedEvent,
                    out nextTempoTime,
                    ref eventShouldBeAdded,
                    out oldTempo);

                PrepareTimeSignatureEventAdding(
                    timedObject as TimedEvent,
                    ref eventShouldBeAdded,
                    out oldTimeSignature);
            }

            if (!eventShouldBeAdded)
                return;

            //

            var playbackEvents = GetPlaybackEvents(timedObject, tempoMap, useNoteEventsDirectly);
            var eventsGroup = new HashSet<RedBlackTreeCoordinate<TimeSpan, PlaybackEvent>>();

            var minTime = TimeSpan.MaxValue;
            var observablePlaybackSource = _playbackSource as ObservablePlaybackSource;

            foreach (var e in playbackEvents)
            {
                RedBlackTreeCoordinate<TimeSpan, PlaybackEvent> node = null;

                if (observablePlaybackSource != null)
                {
                    node = observablePlaybackSource.PlaybackEvents.Add(e.Time, e);
                    e.EventsGroup = eventsGroup;

                    eventsGroup.Add(node);
                }
                else
                    _playbackSource.AddPlaybackEvent(e);

                var noteMetadata = e.NoteMetadata;
                if (noteMetadata != null && _notesMetadataHashSet.Add(noteMetadata))
                {
                    if (!isInitialObject)
                        _notesMetadata.Add(noteMetadata);
                    else
                        _notesMetadata.AddWithoutMaxUpdating(noteMetadata);
                }

                InitializeTrackedData(
                    e.Event,
                    e.RawTime,
                    e.TimedEventMetadata);

                if (!isInitialObject && _hasBeenStarted && e.Time > _clock.CurrentTime && (observablePlaybackSource.PlaybackEventsPosition == null || e.Time < observablePlaybackSource.PlaybackEventsPosition.Key) && e.Time < minTime)
                {
                    observablePlaybackSource.PlaybackEventsPosition = node;
                    minTime = e.Time;
                }
            }

            if (!isInitialObject || calculateTempoMap)
            {
                TryAddSetTempoEvent(
                    timedObject as TimedEvent,
                    nextTempoTime,
                    eventShouldBeAdded,
                    oldTempo);

                TryAddTimeSignatureEvent(
                    timedObject as TimedEvent,
                    eventShouldBeAdded,
                    oldTimeSignature);
            }
        }

        private void TryRemoveSetTempoEvent(
            TimedEvent timedEvent,
            long oldTime)
        {
            if (timedEvent == null)
                return;

            var setTempoEvent = timedEvent.Event as SetTempoEvent;
            if (setTempoEvent == null)
                return;

            var valuesChanges = TempoMap.TempoLine.ToArray();

            Tempo oldTempo = null;
            Tempo newTempo = TempoMap.TempoLine.GetValueAtTime(0);
            TimeSpan? nextTempoTime = null;

            foreach (var valueChange in valuesChanges)
            {
                if (valueChange.Time < oldTime)
                    newTempo = valueChange.Value;
                else if (valueChange.Time == oldTime)
                    oldTempo = valueChange.Value;
                else
                {
                    nextTempoTime = TimeConverter.ConvertTo<MetricTimeSpan>(valueChange.Time, TempoMap);
                    break;
                }
            }

            ScaleTimesAfterTempoChange(
                oldTempo,
                newTempo,
                oldTime,
                nextTempoTime);
        }

        private void PrepareSetTempoEventAdding(
            TimedEvent timedEvent,
            out TimeSpan? nextTempoTime,
            ref bool eventShouldBeAdded,
            out Tempo oldTempo)
        {
            nextTempoTime = null;
            oldTempo = TempoMap.TempoLine.GetValueAtTime(0);

            if (!eventShouldBeAdded)
                return;

            if (timedEvent == null)
                return;

            var setTempoEvent = timedEvent.Event as SetTempoEvent;
            if (setTempoEvent == null)
                return;

            var newTempo = new Tempo(setTempoEvent.MicrosecondsPerQuarterNote);

            var valuesChanges = TempoMap.TempoLine.ToArray();

            foreach (var valueChange in valuesChanges)
            {
                if (valueChange.Time <= timedEvent.Time)
                    oldTempo = valueChange.Value;
                else
                {
                    nextTempoTime = TimeConverter.ConvertTo<MetricTimeSpan>(valueChange.Time, TempoMap);
                    break;
                }
            }

            eventShouldBeAdded = oldTempo != newTempo;
        }

        private void TryAddSetTempoEvent(
            TimedEvent timedEvent,
            TimeSpan? nextTempoTime,
            bool eventShouldBeAdded,
            Tempo oldTempo)
        {
            if (timedEvent == null)
                return;

            var setTempoEvent = timedEvent.Event as SetTempoEvent;
            if (setTempoEvent == null || !eventShouldBeAdded)
                return;

            var newTempo = new Tempo(setTempoEvent.MicrosecondsPerQuarterNote);

            ScaleTimesAfterTempoChange(
                oldTempo,
                newTempo,
                timedEvent.Time,
                nextTempoTime);
        }

        private void ScaleTimesAfterTempoChange(
            Tempo oldTempo,
            Tempo newTempo,
            long tempoChangeMidiTime,
            TimeSpan? nextTempoTime)
        {
            var tempoChangeTime = (TimeSpan)TimeConverter.ConvertTo<MetricTimeSpan>(tempoChangeMidiTime, TempoMap);
            
            var scaleFactor = oldTempo.MicrosecondsPerQuarterNote / (double)newTempo.MicrosecondsPerQuarterNote;
            var shift = nextTempoTime == null
                ? TimeSpan.Zero
                : nextTempoTime.Value - (tempoChangeTime + ScaleTimeSpan(nextTempoTime.Value - tempoChangeTime, scaleFactor));

            ScalePlaybackEventsTimes(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            ScaleNotesMetadataTimes(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            ScaleSnapPointsTimes(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            ScalePlaybackStart(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            ScalePlaybackEnd(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            ScaleCurrentTime(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            TempoMap.TempoLine.SetValue(tempoChangeMidiTime, newTempo);
        }

        private void ScaleCurrentTime(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            ScaleTime(
                () => _clock.CurrentTime,
                time => _clock.SetCurrentTime(time),
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);
        }

        private void ScalePlaybackStart(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            var playbackStart = PlaybackStart;

            if (playbackStart == null || tempoChangeTime > _playbackStartMetric)
                return;

            ScaleTime(
                () => _playbackStartMetric,
                time => _playbackStartMetric = time,
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            if (playbackStart is MetricTimeSpan)
                _playbackStart = (MetricTimeSpan)_playbackStartMetric;
        }

        private void ScalePlaybackEnd(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            var playbackEnd = PlaybackEnd;

            if (playbackEnd == null || tempoChangeTime > _playbackEndMetric)
                return;

            ScaleTime(
                () => _playbackEndMetric,
                time => _playbackEndMetric = time,
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);

            if (playbackEnd is MetricTimeSpan)
                _playbackEnd = (MetricTimeSpan)_playbackEndMetric;
        }

        private void ScaleTime(
            Func<TimeSpan> getTime,
            Action<TimeSpan> setTime,
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            var time = getTime();
            if (tempoChangeTime > time)
                return;

            if (nextTempoTime != null && time > nextTempoTime)
                time -= shift;
            else
                time = tempoChangeTime + ScaleTimeSpan(time - tempoChangeTime, scaleFactor);

            setTime(time);
        }

        private void ScalePlaybackEventsTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            _playbackSource.ScalePlaybackEventsTimes(
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift);
        }

        private void ScaleNotesMetadataTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            ScaleDataAfterTempoChange(
                _notesMetadata,
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift,
                processNode: node =>
                {
                    if (_notesMetadata.UpdateMax(node.TreeNode))
                        _notesMetadata.UpdateMaxUp(node.TreeNode);
                });

            var intersectedNodes = _notesMetadata.Search(tempoChangeTime);

            foreach (var node in intersectedNodes)
            {
                if (_notesMetadata.UpdateMax(node.TreeNode))
                    _notesMetadata.UpdateMaxUp(node.TreeNode);
            }
        }

        private void ScaleSnapPointsTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            ScaleDataAfterTempoChange(
                _snapPoints,
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift,
                (e, time) => e.Time = time);
        }

        private void ScaleDataAfterTempoChange<TValue>(
            RedBlackTree<TimeSpan, TValue> tree,
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift,
            Action<TValue, TimeSpan> updateValueTime = null,
            Action<RedBlackTreeCoordinate<TimeSpan, TValue>> processNode = null)
        {
            var firstNodeAfterTempoChange = tree.GetFirstCoordinateAboveThreshold(tempoChangeTime);
            if (firstNodeAfterTempoChange == null)
                return;

            var node = firstNodeAfterTempoChange;

            do
            {
                if (nextTempoTime != null && node.Key > nextTempoTime)
                    node.Key -= shift;
                else
                    node.Key = tempoChangeTime + ScaleTimeSpan(node.Key - tempoChangeTime, scaleFactor);

                processNode?.Invoke(node);
                updateValueTime?.Invoke(node.Value, node.Key);
            }
            while ((node = tree.GetNextCoordinate(node)) != null);
        }

        private TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleFactor)
        {
            return timeSpan.DivideBy(scaleFactor);
        }

        private void TryRemoveTimeSignatureEvent(
            TimedEvent timedEvent,
            long oldTime)
        {
            if (timedEvent == null)
                return;

            var setTempoEvent = timedEvent.Event as TimeSignatureEvent;
            if (setTempoEvent == null)
                return;

            var newTempo = TempoMap.TimeSignatureLine.GetValueAtTime(oldTime - 1);
            TempoMap.TimeSignatureLine.SetValue(timedEvent.Time, newTempo);
        }

        private void PrepareTimeSignatureEventAdding(
            TimedEvent timedEvent,
            ref bool eventShouldBeAdded,
            out TimeSignature oldTimeSignature)
        {
            oldTimeSignature = TempoMap.TimeSignatureLine.GetValueAtTime(0);

            if (!eventShouldBeAdded)
                return;

            if (timedEvent == null)
                return;

            var timeSignatureEvent = timedEvent.Event as TimeSignatureEvent;
            if (timeSignatureEvent == null)
                return;

            var newTimeSignature = new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator);
            oldTimeSignature = TempoMap.TimeSignatureLine.GetValueAtTime(timedEvent.Time);

            eventShouldBeAdded = oldTimeSignature != newTimeSignature;
        }

        private void TryAddTimeSignatureEvent(
            TimedEvent timedEvent,
            bool eventShouldBeAdded,
            TimeSignature oldTempo)
        {
            if (timedEvent == null)
                return;

            var timeSignatureEvent = timedEvent.Event as TimeSignatureEvent;
            if (timeSignatureEvent == null || !eventShouldBeAdded)
                return;

            var newTempo = new TimeSignature(timeSignatureEvent.Numerator, timeSignatureEvent.Denominator);

            TempoMap.TimeSignatureLine.SetValue(timedEvent.Time, newTempo);
        }

        private bool TryAddNoteEvent(
            ITimedObject timedObject,
            TempoMap tempoMap,
            bool isInitialObject,
            bool useNoteEventsDirectly)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    var noteId = noteEvent.GetNoteId();
                    TimedEvent matchedTimedEvent = null;

                    if (noteEvent is NoteOnEvent)
                    {
                        if (_noteOffEvents.TryGetValue(noteId, out matchedTimedEvent) &&
                            ((NoteEvent)matchedTimedEvent.Event).GetNoteId().Equals(noteId))
                        {
                            _noteOffEvents.TryRemove(noteId, out matchedTimedEvent);
                            AddTimedObject(new Note(timedEvent, matchedTimedEvent, false), TempoMap, isInitialObject, false, useNoteEventsDirectly);
                        }
                        else
                            _noteOnEvents[noteId] = timedEvent;
                    }
                    else
                    {
                        if (_noteOnEvents.TryGetValue(noteId, out matchedTimedEvent) &&
                            ((NoteEvent)matchedTimedEvent.Event).GetNoteId().Equals(noteId))
                        {
                            _noteOnEvents.TryRemove(noteId, out matchedTimedEvent);
                            AddTimedObject(new Note(matchedTimedEvent, timedEvent, false), TempoMap, isInitialObject, false, useNoteEventsDirectly);
                        }
                        else
                            _noteOffEvents[noteId] = timedEvent;
                    }

                    return true;
                }
            }

            return false;
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(
            ITimedObject timedObject,
            TempoMap tempoMap,
            bool useNoteEventsDirectly)
        {
            _playbackEventsBuffer.Clear();

            var customObjectProcessed = false;
            foreach (var e in GetTimedEvents(timedObject))
            {
                _playbackEventsBuffer.Add(GetPlaybackEvent(e, tempoMap));
                customObjectProcessed = true;
            }

            if (customObjectProcessed)
                return _playbackEventsBuffer;

            var chord = timedObject as Chord;
            if (chord != null)
            {
                return GetPlaybackEvents(chord, tempoMap, useNoteEventsDirectly);
            }

            var note = timedObject as Note;
            if (note != null)
            {
                return GetPlaybackEvents(note, tempoMap, note, useNoteEventsDirectly);
            }

            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                _playbackEventsBuffer.Add(GetPlaybackEvent(timedEvent, tempoMap));
                return _playbackEventsBuffer;
            }

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
            {
                _playbackEventsBuffer.AddRange(registeredParameter.GetTimedEvents().Select(e => GetPlaybackEvent(e, tempoMap)));
                return _playbackEventsBuffer;
            }

            return _playbackEventsBuffer;
        }

        private PlaybackEvent GetPlaybackEvent(TimedEvent timedEvent, TempoMap tempoMap)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap, timedEvent);
            playbackEvent.TimedEventMetadata = (timedEvent as IMetadata)?.Metadata;
            return playbackEvent;
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(
            Chord chord,
            TempoMap tempoMap,
            bool useNoteEventsDirectly)
        {
            foreach (var note in chord.Notes)
            {
                foreach (var playbackEvent in GetPlaybackEvents(note, tempoMap, chord, useNoteEventsDirectly))
                {
                    yield return playbackEvent;
                }
            }
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(
            Note note,
            TempoMap tempoMap,
            ITimedObject objectReference,
            bool useNoteEventsDirectly)
        {
            var noteStartTime = new PlaybackTime(note.TimeAs<MetricTimeSpan>(tempoMap));
            var noteEndTime = new PlaybackTime(TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap));
            var noteMetadata = new NotePlaybackEventMetadata(note, noteStartTime, noteEndTime);

            yield return GetPlaybackEventWithNoteMetadata(
                useNoteEventsDirectly ? note.TimedNoteOnEvent : note.GetTimedNoteOnEvent(),
                noteStartTime,
                tempoMap,
                noteMetadata,
                objectReference);

            yield return GetPlaybackEventWithNoteMetadata(
                useNoteEventsDirectly ? note.TimedNoteOffEvent : note.GetTimedNoteOffEvent(),
                noteEndTime,
                tempoMap,
                noteMetadata,
                objectReference);
        }

        private PlaybackEvent GetPlaybackEventWithNoteMetadata(
            TimedEvent timedEvent,
            PlaybackTime time,
            TempoMap tempoMap,
            NotePlaybackEventMetadata noteMetadata,
            ITimedObject objectReference)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap, objectReference, time);
            playbackEvent.NoteMetadata = noteMetadata;
            playbackEvent.TimedEventMetadata = (timedEvent as IMetadata)?.Metadata;
            return playbackEvent;
        }

        private PlaybackEvent CreatePlaybackEvent(
            TimedEvent timedEvent,
            TempoMap tempoMap,
            ITimedObject objectReference,
            PlaybackTime time = null)
        {
            return new PlaybackEvent(
                timedEvent.Event,
                time ?? new PlaybackTime(timedEvent.TimeAs<MetricTimeSpan>(tempoMap)),
                timedEvent.Time,
                objectReference);
        }

        #endregion
    }
}

using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using PlaybackEventsCollection = Melanchall.DryWetMidi.Common.RedBlackTree<System.TimeSpan, Melanchall.DryWetMidi.Multimedia.PlaybackEvent>;

namespace Melanchall.DryWetMidi.Multimedia
{
    public partial class Playback
    {
        #region Fields

        private readonly HashSet<NotePlaybackEventMetadata> _notesMetadataHashSet = new HashSet<NotePlaybackEventMetadata>();

        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOnEvents = new ConcurrentDictionary<NoteId, TimedEvent>();
        private readonly ConcurrentDictionary<NoteId, TimedEvent> _noteOffEvents = new ConcurrentDictionary<NoteId, TimedEvent>();

        private readonly PlaybackEventsCollection _playbackEvents = new PlaybackEventsCollection();
        private readonly object _playbackLockObject = new object();
        private RedBlackTreeCoordinate<TimeSpan, PlaybackEvent> _playbackEventsPosition;
        private bool _beforeStart = true;

        #endregion

        #region Methods

        private void InitializeData(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings)
        {
            InitializePlaybackEvents(timedObjects.GetNotesAndTimedEventsLazy(noteDetectionSettings, true), tempoMap);
            MoveToNextPlaybackEvent();

            var observableTimedObjectsCollection = timedObjects as IObservableTimedObjectsCollection;
            if (observableTimedObjectsCollection != null)
                observableTimedObjectsCollection.CollectionChanged += OnObservableTimedObjectsCollectionChanged;
        }

        private void OnObservableTimedObjectsCollectionChanged(object sender, ObservableTimedObjectsCollectionChangedEventArgs e)
        {
            lock (_playbackLockObject)
            {
                var isRunning = IsRunning;

                var maxTime = TimeSpan.Zero;
                var maxTimeInTicks = 0L;

                if (e.AddedObjects != null)
                {
                    foreach (var obj in e.AddedObjects)
                    {
                        AddNewTimedObject(obj, ref maxTime, ref maxTimeInTicks);
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
                        AddNewTimedObject(obj, ref maxTime, ref maxTimeInTicks);
                    }
                }

                if (maxTime > _duration)
                {
                    _duration = maxTime;
                    _durationInTicks = maxTimeInTicks;
                }
                else
                    UpdateDuration();

                UpdatePlaybackEndMetric();
                UpdatePlaybackStartMetric();

                if (!_playbackEvents.Any())
                {
                    _playbackEventsPosition = null;
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

        private void AddNewTimedObject(ITimedObject timedObject, ref TimeSpan maxTime, ref long maxTimeInTicks)
        {
            AddTimedObject(timedObject, TempoMap, false, ref maxTime, ref maxTimeInTicks);
        }

        private void RemoveTimedObject(ITimedObject timedObject, long oldTime)
        {
            var timedEvent = timedObject as TimedEvent;
            var noteEvent = timedEvent?.Event as NoteEvent;
            var isNoteOnEvent = noteEvent is NoteOnEvent;

            var time = TimeConverter.ConvertTo<MetricTimeSpan>(oldTime, TempoMap);
            var playbackEventsNodes = _playbackEvents
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
                var noteId = GetNoteId(noteEvent);

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

                if (playbackEventNode.Equals(_playbackEventsPosition))
                {
                    _playbackEventsPosition = _playbackEvents.GetNextCoordinate(_playbackEventsPosition);
                    if (_playbackEventsPosition == null)
                        _beforeStart = false;
                }

                _playbackEvents.Remove(playbackEventNode);

                var noteMetadata = playbackEvent.Metadata.Note;
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

        private void InitializePlaybackEvents(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
        {
            var maxTime = TimeSpan.Zero;
            var maxTimeInTicks = 0L;

            foreach (var timedObject in timedObjects)
            {
                AddTimedObject(timedObject, tempoMap, true, ref maxTime, ref maxTimeInTicks);
            }
        }

        private void AddTimedObject(
            ITimedObject timedObject,
            TempoMap tempoMap,
            bool isInitialObject,
            ref TimeSpan maxTime,
            ref long maxTimeInTicks)
        {
            if (TryAddNoteEvent(timedObject, tempoMap, isInitialObject, ref maxTime, ref maxTimeInTicks))
                return;

            //

            TimeSpan? nextTempoTime = null;
            bool eventShouldBeAdded = true;
            Tempo oldTempo;
            TimeSignature oldTimeSignature;

            PrepareSetTempoEventAdding(
                timedObject as TimedEvent,
                out nextTempoTime,
                ref eventShouldBeAdded,
                out oldTempo);

            PrepareTimeSignatureEventAdding(
                timedObject as TimedEvent,
                ref eventShouldBeAdded,
                out oldTimeSignature);

            if (!eventShouldBeAdded)
                return;

            //

            var playbackEvents = GetPlaybackEvents(timedObject, tempoMap);
            var eventsGroup = new HashSet<RedBlackTreeCoordinate<TimeSpan, PlaybackEvent>>();

            var minTime = TimeSpan.MaxValue;

            foreach (var e in playbackEvents)
            {
                var node = _playbackEvents.Add(e.Time, e);
                e.EventsGroup = eventsGroup;

                eventsGroup.Add(node);

                var noteMetadata = e.Metadata.Note;
                if (noteMetadata != null && _notesMetadataHashSet.Add(noteMetadata))
                    _notesMetadata.Add(noteMetadata.StartTime, noteMetadata);

                InitializeTrackedData(
                    e.Event,
                    e.RawTime,
                    e.Metadata.TimedEvent.Metadata);

                if (!isInitialObject && _hasBeenStarted && e.Time > _clock.CurrentTime && (_playbackEventsPosition == null || e.Time < _playbackEventsPosition.Key) && e.Time < minTime)
                {
                    _playbackEventsPosition = node;
                    minTime = e.Time;
                }

                if (e.Time > maxTime)
                {
                    maxTime = e.Time;
                    maxTimeInTicks = e.RawTime;
                }
            }

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

        private NoteId GetNoteId(NoteEvent noteEvent)
        {
            return new NoteId(noteEvent.Channel, noteEvent.NoteNumber);
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

            for (var i = 0; i < valuesChanges.Length; i++)
            {
                var valueChange = valuesChanges[i];
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

            for (var i = 0; i < valuesChanges.Length; i++)
            {
                var valueChange = valuesChanges[i];
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

            // TODO: more tests
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
            ScaleDataAfterTempoChange(
                _playbackEvents,
                tempoChangeTime,
                nextTempoTime,
                scaleFactor,
                shift,
                (e, time) => e.Time.Time = time);
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
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks / scaleFactor));
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

            // TODO: optimize

            var valuesChanges = TempoMap.TimeSignatureLine.ToArray();

            var newTempo = TempoMap.TimeSignatureLine.GetValueAtTime(0);

            for (var i = 0; i < valuesChanges.Length; i++)
            {
                var valueChange = valuesChanges[i];
                if (valueChange.Time < oldTime)
                    newTempo = valueChange.Value;
                else
                    break;
            }

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

            var valuesChanges = TempoMap.TimeSignatureLine.ToArray();

            // TODO: optimize

            for (var i = 0; i < valuesChanges.Length; i++)
            {
                var valueChange = valuesChanges[i];
                if (valueChange.Time <= timedEvent.Time)
                    oldTimeSignature = valueChange.Value;
            }

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
            ref TimeSpan maxTime,
            ref long maxTimeInTicks)
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
                            AddTimedObject(new Note(timedEvent, matchedTimedEvent, false), TempoMap, isInitialObject, ref maxTime, ref maxTimeInTicks);
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
                            AddTimedObject(new Note(matchedTimedEvent, timedEvent, false), TempoMap, isInitialObject, ref maxTime, ref maxTimeInTicks);
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
            var noteStartTime = new PlaybackTime(note.TimeAs<MetricTimeSpan>(tempoMap));
            var noteEndTime = new PlaybackTime(TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap));
            var noteMetadata = new NotePlaybackEventMetadata(note, noteStartTime, noteEndTime);

            yield return GetPlaybackEventWithNoteMetadata(
                note.GetTimedNoteOnEvent(),
                noteStartTime,
                tempoMap,
                noteMetadata,
                objectReference);

            yield return GetPlaybackEventWithNoteMetadata(
                note.GetTimedNoteOffEvent(),
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
            playbackEvent.Metadata.Note = noteMetadata;
            playbackEvent.Metadata.TimedEvent = new TimedEventPlaybackEventMetadata((timedEvent as IMetadata)?.Metadata);
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

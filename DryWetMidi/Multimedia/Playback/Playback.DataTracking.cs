using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    public partial class Playback
    {
        #region Nested enums

        [Flags]
        private enum TrackedParameterType
        {
            Program = 1 << 0,
            PitchValue = 1 << 1,
            ControlValue = 1 << 2,
            ChannelAftertouch = 1 << 3,
            NoteAftertouch = 1 << 4,

            All = Program | PitchValue | ControlValue | ChannelAftertouch | NoteAftertouch
        }

        #endregion

        #region Nested classes

        private sealed class EventWithMetadata
        {
            public EventWithMetadata(MidiEvent midiEvent, object metadata)
            {
                Event = midiEvent;
                Metadata = metadata;
            }

            public MidiEvent Event { get; }

            public object Metadata { get; }
        }

        private sealed class DataChange<TData> : IMetadata
        {
            public DataChange(TData data, object metadata)
            {
                Data = data;
                Metadata = metadata;
            }

            public DataChange(TData data, object metadata, bool isDefault)
                : this(data, metadata)
            {
                IsDefault = isDefault;
            }

            public TData Data { get; }

            public object Metadata { get; set; }

            public bool IsDefault { get; }

            public override bool Equals(object obj)
            {
                var other = obj as DataChange<TData>;
                return other != null &&
                       Data.Equals(other.Data) &&
                       IsDefault == other.IsDefault;
            }

            public override int GetHashCode()
            {
                return Data.GetHashCode();
            }
        }

        private sealed class DataChangesManager<TData, TEvent>
            where TData : struct
            where TEvent : ChannelEvent, new()
        {
            private readonly Action<TEvent, TData> _setEventData;
            private readonly Func<TEvent, TData> _getEventData;

            public DataChangesManager(
                TData defaultValue,
                Action<TEvent, TData> setEventData,
                Func<TEvent, TData> getEventData)
            {
                DefaultChange = new DataChange<TData>(defaultValue, null, true);

                _setEventData = setEventData;
                _getEventData = getEventData;
            }

            public bool IsEnabled { get; set; } = true;

            public DataChange<TData>[] CurrentChangesByChannel { get; } = new DataChange<TData>[FourBitNumber.MaxValue + 1];

            public RedBlackTree<long, DataChange<TData>>[] ChangesTreesByChannel { get; } = FourBitNumber.Values
                .Select(n => new RedBlackTree<long, DataChange<TData>>())
                .ToArray();

            public DataChange<TData> DefaultChange { get; }

            public void Clear()
            {
                foreach (var channel in FourBitNumber.Values)
                {
                    CurrentChangesByChannel[channel] = null;
                    ChangesTreesByChannel[channel].Clear();
                }
            }

            public IEnumerable<EventWithMetadata> GetEventsAtTime(long time)
            {
                if (!IsEnabled)
                    yield break;

                foreach (var channel in FourBitNumber.Values)
                {
                    var tree = ChangesTreesByChannel[channel];
                    var node = tree.GetLastCoordinateBelowThreshold(time + 1);
                    if (node?.Key == time)
                        continue;

                    var changeAtTime = node?.Value ?? DefaultChange;

                    var currentChange = CurrentChangesByChannel[channel];
                    if (!changeAtTime.Data.Equals(currentChange?.Data) && (currentChange != null || !changeAtTime.IsDefault))
                    {
                        var midiEvent = new TEvent();
                        _setEventData(midiEvent, changeAtTime.Data);
                        midiEvent.Channel = channel;
                        yield return new EventWithMetadata(
                            midiEvent,
                            changeAtTime.Metadata);
                    }
                }
            }

            public void InitializeData(
                TEvent midiEvent,
                long time,
                object metadata)
            {
                if (midiEvent == null)
                    return;

                var tree = ChangesTreesByChannel[midiEvent.Channel];
                tree.Add(time, new DataChange<TData>(_getEventData(midiEvent), metadata));
            }

            public void UpdateCurrentData(TEvent midiEvent, object metadata)
            {
                if (midiEvent == null)
                    return;

                CurrentChangesByChannel[midiEvent.Channel] = new DataChange<TData>(_getEventData(midiEvent), metadata);
            }

            public void RemoveData(TEvent midiEvent, long time)
            {
                if (midiEvent == null)
                    return;

                var tree = ChangesTreesByChannel[midiEvent.Channel];
                var nodes = tree.GetCoordinatesByKey(time);

                var programChange = new DataChange<TData>(_getEventData(midiEvent), null);

                foreach (var node in nodes)
                {
                    if (node.Value.Equals(programChange))
                        tree.Remove(node);
                }
            }
        }

        private sealed class DataChangesManager<TKey, TData, TEvent>
            where TData : struct
            where TEvent : ChannelEvent, new()
        {
            private readonly Action<TEvent, TData> _setEventData;
            private readonly Func<TEvent, TData> _getEventData;
            private readonly Action<TEvent, TKey> _setEventKey;
            private readonly Func<TEvent, TKey> _getEventKey;

            public DataChangesManager(
                TData defaultValue,
                Action<TEvent, TData> setEventData,
                Func<TEvent, TData> getEventData,
                Action<TEvent, TKey> setEventKey,
                Func<TEvent, TKey> getEventKey)
            {
                DefaultChange = new DataChange<TData>(defaultValue, null, true);

                _setEventData = setEventData;
                _getEventData = getEventData;
                _setEventKey = setEventKey;
                _getEventKey = getEventKey;
            }

            public bool IsEnabled { get; set; } = true;

            public Dictionary<TKey, DataChangesManager<TData, TEvent>> ChangesManagersByKeys { get; } = new Dictionary<TKey, DataChangesManager<TData, TEvent>>();

            public DataChange<TData> DefaultChange { get; }

            public void Clear()
            {
                ChangesManagersByKeys.Clear();
            }

            public IEnumerable<EventWithMetadata> GetEventsAtTime(long time)
            {
                if (!IsEnabled)
                    yield break;

                foreach (var keyAndchangesManager in ChangesManagersByKeys)
                {
                    foreach (var e in keyAndchangesManager.Value.GetEventsAtTime(time))
                    {
                        _setEventKey((TEvent)e.Event, keyAndchangesManager.Key);
                        yield return e;
                    }
                }
            }

            public void InitializeData(
                TEvent midiEvent,
                long time,
                object metadata)
            {
                if (midiEvent == null)
                    return;

                var key = _getEventKey(midiEvent);
                if (!ChangesManagersByKeys.TryGetValue(key, out var changesManager))
                    ChangesManagersByKeys.Add(key, changesManager = new DataChangesManager<TData, TEvent>(DefaultChange.Data, _setEventData, _getEventData));

                changesManager.InitializeData(midiEvent, time, metadata);
            }

            public void UpdateCurrentData(TEvent midiEvent, object metadata)
            {
                if (midiEvent == null)
                    return;

                var key = _getEventKey(midiEvent);
                if (!ChangesManagersByKeys.TryGetValue(key, out var changesManager))
                    ChangesManagersByKeys.Add(key, changesManager = new DataChangesManager<TData, TEvent>(DefaultChange.Data, _setEventData, _getEventData));

                changesManager.UpdateCurrentData(midiEvent, metadata);
            }

            public void RemoveData(TEvent midiEvent, long time)
            {
                if (midiEvent == null)
                    return;

                var key = _getEventKey(midiEvent);
                if (!ChangesManagersByKeys.TryGetValue(key, out var changesManager))
                    return;

                changesManager.RemoveData(midiEvent, time);
            }
        }

        #endregion

        #region Fields

        private readonly DataChangesManager<SevenBitNumber, ProgramChangeEvent> _programChangesManager = new DataChangesManager<SevenBitNumber, ProgramChangeEvent>(
            SevenBitNumber.MinValue,
            (e, d) => e.ProgramNumber = d,
            e => e.ProgramNumber);
        
        private readonly DataChangesManager<ushort, PitchBendEvent> _pitchBendChangesManager = new DataChangesManager<ushort, PitchBendEvent>(
            PitchBendEvent.DefaultPitchValue,
            (e, d) => e.PitchValue = d,
            e => e.PitchValue);
        
        private readonly DataChangesManager<SevenBitNumber, ChannelAftertouchEvent> _channelAftertouchChangesManager = new DataChangesManager<SevenBitNumber, ChannelAftertouchEvent>(
            SevenBitNumber.MinValue,
            (e, d) => e.AftertouchValue = d,
            e => e.AftertouchValue);

        private readonly DataChangesManager<SevenBitNumber, SevenBitNumber, ControlChangeEvent> _controlsChangesManager = new DataChangesManager<SevenBitNumber, SevenBitNumber, ControlChangeEvent>(
            SevenBitNumber.MinValue,
            (e, d) => e.ControlValue = d,
            e => e.ControlValue,
            (e, d) => e.ControlNumber = d,
            e => e.ControlNumber);

        private readonly DataChangesManager<SevenBitNumber, SevenBitNumber, NoteAftertouchEvent> _noteAftertouchChangesManager = new DataChangesManager<SevenBitNumber, SevenBitNumber, NoteAftertouchEvent>(
            SevenBitNumber.MinValue,
            (e, d) => e.AftertouchValue = d,
            e => e.AftertouchValue,
            (e, d) => e.NoteNumber = d,
            e => e.NoteNumber);


        private Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>> _getParameterEventsAtTime;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether program must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="ProgramChangeEvent"/> corresponding to the program at new time,
        /// if needed. The default value is <c>true</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackProgram
        {
            get { return _programChangesManager.IsEnabled; }
            set
            {
                if (_programChangesManager.IsEnabled == value)
                    return;

                _programChangesManager.IsEnabled = value;

                if (value)
                    SendTrackedData(TrackedParameterType.Program);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pitch value must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="PitchBendEvent"/> corresponding to the pitch value at new time,
        /// if needed. The default value is <c>true</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackPitchValue
        {
            get { return _pitchBendChangesManager.IsEnabled; }
            set
            {
                if (_pitchBendChangesManager.IsEnabled == value)
                    return;

                _pitchBendChangesManager.IsEnabled = value;

                if (value)
                    SendTrackedData(TrackedParameterType.PitchValue);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether controller values must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="ControlChangeEvent"/> corresponding to the controller value at new time,
        /// if needed. The default value is <c>true</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackControlValue
        {
            get { return _controlsChangesManager.IsEnabled; }
            set
            {
                if (_controlsChangesManager.IsEnabled == value)
                    return;

                _controlsChangesManager.IsEnabled = value;

                if (value)
                    SendTrackedData(TrackedParameterType.ControlValue);
            }
        }

        public bool TrackChannelAftertouch
        {
            get { return _channelAftertouchChangesManager.IsEnabled; }
            set
            {
                if (_channelAftertouchChangesManager.IsEnabled == value)
                    return;

                _channelAftertouchChangesManager.IsEnabled = value;

                if (value)
                    SendTrackedData(TrackedParameterType.ChannelAftertouch);
            }
        }

        public bool TrackNoteAftertouch
        {
            get { return _noteAftertouchChangesManager.IsEnabled; }
            set
            {
                if (_noteAftertouchChangesManager.IsEnabled == value)
                    return;

                _noteAftertouchChangesManager.IsEnabled = value;

                if (value)
                    SendTrackedData(TrackedParameterType.NoteAftertouch);
            }
        }

        #endregion

        #region Methods

        private void ClearTrackedData()
        {
            _programChangesManager.Clear();
            _pitchBendChangesManager.Clear();
            _channelAftertouchChangesManager.Clear();
            _controlsChangesManager.Clear();
            _noteAftertouchChangesManager.Clear();
        }

        private void InitializeDataTracking()
        {
            _getParameterEventsAtTime = new Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>>
            {
                [TrackedParameterType.Program] = _programChangesManager.GetEventsAtTime,
                [TrackedParameterType.PitchValue] = _pitchBendChangesManager.GetEventsAtTime,
                [TrackedParameterType.ControlValue] = _controlsChangesManager.GetEventsAtTime,
                [TrackedParameterType.ChannelAftertouch] = _channelAftertouchChangesManager.GetEventsAtTime,
                [TrackedParameterType.NoteAftertouch] = _noteAftertouchChangesManager.GetEventsAtTime,
            };
        }

        private void InitializeTrackedData(MidiEvent midiEvent, long time, object metadata)
        {
            _programChangesManager.InitializeData(midiEvent as ProgramChangeEvent, time, metadata);
            _pitchBendChangesManager.InitializeData(midiEvent as PitchBendEvent, time, metadata);
            _controlsChangesManager.InitializeData(midiEvent as ControlChangeEvent, time, metadata);
            _channelAftertouchChangesManager.InitializeData(midiEvent as ChannelAftertouchEvent, time, metadata);
            _noteAftertouchChangesManager.InitializeData(midiEvent as NoteAftertouchEvent, time, metadata);
        }

        private void UpdateCurrentTrackedData(MidiEvent midiEvent, object metadata)
        {
            _programChangesManager.UpdateCurrentData(midiEvent as ProgramChangeEvent, metadata);
            _pitchBendChangesManager.UpdateCurrentData(midiEvent as PitchBendEvent, metadata);
            _controlsChangesManager.UpdateCurrentData(midiEvent as ControlChangeEvent, metadata);
            _channelAftertouchChangesManager.UpdateCurrentData(midiEvent as ChannelAftertouchEvent, metadata);
            _noteAftertouchChangesManager.UpdateCurrentData(midiEvent as NoteAftertouchEvent, metadata);
        }

        private void RemoveTrackedData(MidiEvent midiEvent, long time)
        {
            _programChangesManager.RemoveData(midiEvent as ProgramChangeEvent, time);
            _pitchBendChangesManager.RemoveData(midiEvent as PitchBendEvent, time);
            _controlsChangesManager.RemoveData(midiEvent as ControlChangeEvent, time);
            _channelAftertouchChangesManager.RemoveData(midiEvent as ChannelAftertouchEvent, time);
            _noteAftertouchChangesManager.RemoveData(midiEvent as NoteAftertouchEvent, time);
        }

        private void SendTrackedData(TrackedParameterType trackedParameterType = TrackedParameterType.All)
        {
            foreach (var eventWithMetadata in GetEventsAtTime(_clock.CurrentTime, trackedParameterType))
            {
                PlayEvent(eventWithMetadata.Event, eventWithMetadata.Metadata);
            }
        }

        private IEnumerable<EventWithMetadata> GetEventsAtTime(TimeSpan time, TrackedParameterType trackedParameterType)
        {
            var convertedTime = TimeConverter.ConvertFrom((MetricTimeSpan)time, TempoMap);

            foreach (var getEvents in _getParameterEventsAtTime)
            {
                if (trackedParameterType.HasFlag(getEvents.Key))
                {
                    foreach (var e in getEvents.Value(convertedTime))
                    {
                        yield return e;
                    }
                }
            }
        }

        #endregion
    }
}

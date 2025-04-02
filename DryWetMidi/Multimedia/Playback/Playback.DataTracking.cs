﻿using Melanchall.DryWetMidi.Common;
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

            All = Program | PitchValue | ControlValue
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

        private abstract class DataChange<TData> : IMetadata
        {
            protected DataChange(TData data, object metadata)
            {
                Data = data;
                Metadata = metadata;
            }

            protected DataChange(TData data, object metadata, bool isDefault)
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

        private sealed class ProgramChange : DataChange<SevenBitNumber>
        {
            public ProgramChange(SevenBitNumber programNumber, object metadata)
                : base(programNumber, metadata)
            {
            }

            public ProgramChange(SevenBitNumber programNumber, object metadata, bool isDefault)
                : base(programNumber, metadata, isDefault)
            {
            }
        }

        private sealed class PitchValueChange : DataChange<ushort>
        {
            public PitchValueChange(ushort pitchValue, object metadata)
                : base(pitchValue, metadata)
            {
            }

            public PitchValueChange(ushort pitchValue, object metadata, bool isDefault)
                : base(pitchValue, metadata, isDefault)
            {
            }
        }

        private sealed class ControlValueChange : DataChange<SevenBitNumber>
        {
            public ControlValueChange(SevenBitNumber controlValue, object metadata)
                : base(controlValue, metadata)
            {
            }

            public ControlValueChange(SevenBitNumber controlValue, object metadata, bool isDefault)
                : base(controlValue, metadata, isDefault)
            {
            }
        }

        #endregion

        #region Constants

        private static readonly ProgramChange DefaultProgramChange = new ProgramChange(SevenBitNumber.MinValue, null, true);
        private static readonly PitchValueChange DefaultPitchValueChange = new PitchValueChange(PitchBendEvent.DefaultPitchValue, null, true);
        private static readonly ControlValueChange DefaultControlValueChange = new ControlValueChange(SevenBitNumber.MinValue, null, true);

        #endregion

        #region Fields

        private readonly ProgramChange[] _currentProgramChanges = new ProgramChange[FourBitNumber.MaxValue + 1];
        private readonly RedBlackTree<long, ProgramChange>[] _programChangesTreesByChannel = FourBitNumber.Values
            .Select(n => new RedBlackTree<long, ProgramChange>())
            .ToArray();

        private readonly PitchValueChange[] _currentPitchValues = new PitchValueChange[FourBitNumber.MaxValue + 1];
        private readonly RedBlackTree<long, PitchValueChange>[] _pitchValuesTreesByChannel = FourBitNumber.Values
            .Select(n => new RedBlackTree<long, PitchValueChange>())
            .ToArray();

        private readonly Dictionary<SevenBitNumber, ControlValueChange>[] _currentControlsValuesChangesByChannel = new Dictionary<SevenBitNumber, ControlValueChange>[FourBitNumber.MaxValue + 1];
        private readonly Dictionary<SevenBitNumber, RedBlackTree<long, ControlValueChange>>[] _controlsValuesChangesTreesByChannel = FourBitNumber.Values
            .Select(n => new Dictionary<SevenBitNumber, RedBlackTree<long, ControlValueChange>>())
            .ToArray();

        private Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>> _getParameterEventsAtTime;

        private bool _trackProgram = true;
        private bool _trackPitchValue = true;
        private bool _trackControlValue = true;

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
            get { return _trackProgram; }
            set
            {
                if (_trackProgram == value)
                    return;

                _trackProgram = value;

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
            get { return _trackPitchValue; }
            set
            {
                if (_trackPitchValue == value)
                    return;

                _trackPitchValue = value;

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
            get { return _trackControlValue; }
            set
            {
                if (_trackControlValue == value)
                    return;

                _trackControlValue = value;

                if (value)
                    SendTrackedData(TrackedParameterType.ControlValue);
            }
        }

        #endregion

        #region Methods

        private void InitializeDataTracking()
        {
            _getParameterEventsAtTime = new Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>>
            {
                [TrackedParameterType.Program] = GetProgramChangeEventsAtTime,
                [TrackedParameterType.PitchValue] = GetPitchBendEventsAtTime,
                [TrackedParameterType.ControlValue] = GetControlChangeEventsAtTime
            };
        }

        private void InitializeTrackedData(MidiEvent midiEvent, long time, object metadata)
        {
            InitializeProgramChangeData(midiEvent as ProgramChangeEvent, time, metadata);
            InitializePitchBendData(midiEvent as PitchBendEvent, time, metadata);
            InitializeControlData(midiEvent as ControlChangeEvent, time, metadata);
        }

        private void UpdateCurrentTrackedData(MidiEvent midiEvent, object metadata)
        {
            UpdateCurrentProgramChangeData(midiEvent as ProgramChangeEvent, metadata);
            UpdateCurrentPitchBendData(midiEvent as PitchBendEvent, metadata);
            UpdateCurrentControlData(midiEvent as ControlChangeEvent, metadata);
        }

        private void RemoveTrackedData(MidiEvent midiEvent, long time)
        {
            RemoveProgramChangeData(midiEvent as ProgramChangeEvent, time);
            RemovePitchBendData(midiEvent as PitchBendEvent, time);
            RemoveControlData(midiEvent as ControlChangeEvent, time);
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

        private void UpdateCurrentProgramChangeData(ProgramChangeEvent programChangeEvent, object metadata)
        {
            if (programChangeEvent == null)
                return;

            _currentProgramChanges[programChangeEvent.Channel] = new ProgramChange(programChangeEvent.ProgramNumber, metadata);
        }

        private void InitializeProgramChangeData(ProgramChangeEvent programChangeEvent, long time, object metadata)
        {
            if (programChangeEvent == null)
                return;

            var tree = _programChangesTreesByChannel[programChangeEvent.Channel];
            tree.Add(time, new ProgramChange(programChangeEvent.ProgramNumber, metadata));
        }

        private void RemoveProgramChangeData(ProgramChangeEvent programChangeEvent, long time)
        {
            if (programChangeEvent == null)
                return;

            var tree = _programChangesTreesByChannel[programChangeEvent.Channel];
            var nodes = tree.GetCoordinatesByKey(time);

            var programChange = new ProgramChange(programChangeEvent.ProgramNumber, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(programChange))
                    tree.Remove(node);
            }
        }

        private IEnumerable<EventWithMetadata> GetProgramChangeEventsAtTime(long time)
        {
            if (!TrackProgram)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var tree = _programChangesTreesByChannel[channel];
                var node = tree.GetLastCoordinateBelowThreshold(time + 1);
                if (node?.Key == time)
                    continue;

                var programChangeAtTime = node?.Value ?? DefaultProgramChange;

                var currentProgramChange = _currentProgramChanges[channel];
                if (programChangeAtTime.Data != currentProgramChange?.Data && (currentProgramChange != null || !programChangeAtTime.IsDefault))
                    yield return new EventWithMetadata(
                        new ProgramChangeEvent(programChangeAtTime.Data) { Channel = channel },
                        programChangeAtTime.Metadata);
            }
        }

        private void UpdateCurrentPitchBendData(PitchBendEvent pitchBendEvent, object metadata)
        {
            if (pitchBendEvent == null)
                return;

            _currentPitchValues[pitchBendEvent.Channel] = new PitchValueChange(pitchBendEvent.PitchValue, metadata);
        }

        private void InitializePitchBendData(PitchBendEvent pitchBendEvent, long time, object metadata)
        {
            if (pitchBendEvent == null)
                return;

            var tree = _pitchValuesTreesByChannel[pitchBendEvent.Channel];
            tree.Add(time, new PitchValueChange(pitchBendEvent.PitchValue, metadata));
        }

        private void RemovePitchBendData(PitchBendEvent pitchBendEvent, long time)
        {
            if (pitchBendEvent == null)
                return;

            var tree = _pitchValuesTreesByChannel[pitchBendEvent.Channel];
            var nodes = tree.GetCoordinatesByKey(time);

            var pitchBend = new PitchValueChange(pitchBendEvent.PitchValue, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(pitchBend))
                    tree.Remove(node);
            }
        }

        private IEnumerable<EventWithMetadata> GetPitchBendEventsAtTime(long time)
        {
            if (!TrackPitchValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var tree = _pitchValuesTreesByChannel[channel];
                var node = tree.GetLastCoordinateBelowThreshold(time + 1);
                if (node?.Key == time)
                    continue;

                var pitchValueChangeAtTime = node?.Value ?? DefaultPitchValueChange;

                var currentPitchValueChange = _currentPitchValues[channel];
                if (pitchValueChangeAtTime.Data != currentPitchValueChange?.Data && (currentPitchValueChange != null || !pitchValueChangeAtTime.IsDefault))
                    yield return new EventWithMetadata(
                        new PitchBendEvent(pitchValueChangeAtTime.Data) { Channel = channel },
                        pitchValueChangeAtTime.Metadata);
            }
        }

        private void UpdateCurrentControlData(ControlChangeEvent controlChangeEvent, object metadata)
        {
            if (controlChangeEvent == null)
                return;

            var controlsCurrentValues = _currentControlsValuesChangesByChannel[controlChangeEvent.Channel];
            if (controlsCurrentValues == null)
                controlsCurrentValues = _currentControlsValuesChangesByChannel[controlChangeEvent.Channel] = new Dictionary<SevenBitNumber, ControlValueChange>();

            controlsCurrentValues[controlChangeEvent.ControlNumber] = new ControlValueChange(controlChangeEvent.ControlValue, metadata);
        }

        private void InitializeControlData(ControlChangeEvent controlChangeEvent, long time, object metadata)
        {
            if (controlChangeEvent == null)
                return;

            var trees = _controlsValuesChangesTreesByChannel[controlChangeEvent.Channel];

            RedBlackTree<long, ControlValueChange> tree;
            if (!trees.TryGetValue(controlChangeEvent.ControlNumber, out tree))
                trees.Add(controlChangeEvent.ControlNumber, tree = new RedBlackTree<long, ControlValueChange>());

            tree.Add(time, new ControlValueChange(controlChangeEvent.ControlValue, metadata));
        }

        private void RemoveControlData(ControlChangeEvent controlChangeEvent, long time)
        {
            if (controlChangeEvent == null)
                return;

            var trees = _controlsValuesChangesTreesByChannel[controlChangeEvent.Channel];

            RedBlackTree<long, ControlValueChange> tree;
            if (!trees.TryGetValue(controlChangeEvent.ControlNumber, out tree))
                trees.Add(controlChangeEvent.ControlNumber, tree = new RedBlackTree<long, ControlValueChange>());

            var nodes = tree.GetCoordinatesByKey(time);

            var controlValueChange = new ControlValueChange(controlChangeEvent.ControlValue, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(controlValueChange))
                    tree.Remove(node);
            }
        }

        private IEnumerable<EventWithMetadata> GetControlChangeEventsAtTime(long time)
        {
            if (!TrackControlValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var controlsValuesChangesTreesByControlNumber = _controlsValuesChangesTreesByChannel[channel];
                var currentControlsValuesChangesByControlNumber = _currentControlsValuesChangesByChannel[channel];

                foreach (var controlNumber in SevenBitNumber.Values)
                {
                    RedBlackTree<long, ControlValueChange> tree;
                    if (!controlsValuesChangesTreesByControlNumber.TryGetValue(controlNumber, out tree))
                        continue;

                    var node = tree.GetLastCoordinateBelowThreshold(time + 1);
                    if (node?.Key == time)
                        continue;

                    var controlValueChangeAtTime = node?.Value ?? DefaultControlValueChange;

                    ControlValueChange currentControlValueChange = null;
                    if (currentControlsValuesChangesByControlNumber != null)
                        currentControlsValuesChangesByControlNumber.TryGetValue(controlNumber, out currentControlValueChange);

                    if (controlValueChangeAtTime.Data != currentControlValueChange?.Data && (currentControlValueChange != null || !controlValueChangeAtTime.IsDefault))
                        yield return new EventWithMetadata(
                            new ControlChangeEvent(controlNumber, controlValueChangeAtTime.Data) { Channel = channel },
                            controlValueChangeAtTime.Metadata);
                }
            }
        }

        #endregion
    }
}

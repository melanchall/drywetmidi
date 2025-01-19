using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class PlaybackDataTracker
    {
        #region Nested enums

        [Flags]
        public enum TrackedParameterType
        {
            Program = 1 << 0,
            PitchValue = 1 << 1,
            ControlValue = 1 << 2,

            All = Program | PitchValue | ControlValue
        }

        #endregion

        #region Nested classes

        public sealed class EventWithMetadata
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
        }

        private sealed class ProgramChange : DataChange<SevenBitNumber>, IEquatable<ProgramChange>
        {
            public ProgramChange(SevenBitNumber programNumber, object metadata)
                : base(programNumber, metadata)
            {
            }

            public ProgramChange(SevenBitNumber programNumber, object metadata, bool isDefault)
                : base(programNumber, metadata, isDefault)
            {
            }

            public bool Equals(ProgramChange other)
            {
                return other != null &&
                       Data == other.Data &&
                       IsDefault == other.IsDefault;
            }
        }

        private sealed class PitchValueChange : DataChange<ushort>, IEquatable<PitchValueChange>
        {
            public PitchValueChange(ushort pitchValue, object metadata)
                : base(pitchValue, metadata)
            {
            }

            public PitchValueChange(ushort pitchValue, object metadata, bool isDefault)
                : base(pitchValue, metadata, isDefault)
            {
            }

            public bool Equals(PitchValueChange other)
            {
                return other != null &&
                       Data == other.Data &&
                       IsDefault == other.IsDefault;
            }
        }

        private sealed class ControlValueChange : DataChange<SevenBitNumber>, IEquatable<ControlValueChange>
        {
            public ControlValueChange(SevenBitNumber controlValue, object metadata)
                : base(controlValue, metadata)
            {
            }

            public ControlValueChange(SevenBitNumber controlValue, object metadata, bool isDefault)
                : base(controlValue, metadata, isDefault)
            {
            }

            public bool Equals(ControlValueChange other)
            {
                return other != null &&
                       Data == other.Data &&
                       IsDefault == other.IsDefault;
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

        private readonly TempoMap _tempoMap;
        private readonly Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>> _getParameterEventsAtTime;

        #endregion

        #region Constructor

        public PlaybackDataTracker(TempoMap tempoMap)
        {
            _tempoMap = tempoMap;

            _getParameterEventsAtTime = new Dictionary<TrackedParameterType, Func<long, IEnumerable<EventWithMetadata>>>
            {
                [TrackedParameterType.Program] = time => GetProgramChangeEventsAtTime(time),
                [TrackedParameterType.PitchValue] = time => GetPitchBendEventsAtTime(time),
                [TrackedParameterType.ControlValue] = time => GetControlChangeEventsAtTime(time)
            };
        }

        #endregion

        #region Properties

        public bool TrackProgram { get; set; } = true;

        public bool TrackPitchValue { get; set; }

        public bool TrackControlValue { get; set; }

        #endregion

        #region Methods

        public void InitializeData(MidiEvent midiEvent, long time, object metadata)
        {
            InitializeProgramChangeData(midiEvent as ProgramChangeEvent, time, metadata);
            InitializePitchBendData(midiEvent as PitchBendEvent, time, metadata);
            InitializeControlData(midiEvent as ControlChangeEvent, time, metadata);
        }

        public void UpdateCurrentData(MidiEvent midiEvent, object metadata)
        {
            UpdateCurrentProgramChangeData(midiEvent as ProgramChangeEvent, metadata);
            UpdateCurrentPitchBendData(midiEvent as PitchBendEvent, metadata);
            UpdateCurrentControlData(midiEvent as ControlChangeEvent, metadata);
        }

        public void RemoveData(MidiEvent midiEvent, long time)
        {
            RemoveProgramChangeData(midiEvent as ProgramChangeEvent, time);
            RemovePitchBendData(midiEvent as PitchBendEvent, time);
            RemoveControlData(midiEvent as ControlChangeEvent, time);
        }

        public IEnumerable<EventWithMetadata> GetEventsAtTime(TimeSpan time, TrackedParameterType trackedParameterType)
        {
            var convertedTime = TimeConverter.ConvertFrom((MetricTimeSpan)time, _tempoMap);

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
            var nodes = tree.SearchNodes(time);

            var programChange = new ProgramChange(programChangeEvent.ProgramNumber, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(programChange))
                    tree.Delete(node);
            }
        }

        private IEnumerable<EventWithMetadata> GetProgramChangeEventsAtTime(long time)
        {
            if (!TrackProgram)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var tree = _programChangesTreesByChannel[channel];
                var node = tree.GetLastNodeBelowThreshold(time + 1);
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
            var nodes = tree.SearchNodes(time);

            var pitchBend = new PitchValueChange(pitchBendEvent.PitchValue, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(pitchBend))
                    tree.Delete(node);
            }
        }

        private IEnumerable<EventWithMetadata> GetPitchBendEventsAtTime(long time)
        {
            if (!TrackPitchValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var tree = _pitchValuesTreesByChannel[channel];
                var node = tree.GetLastNodeBelowThreshold(time + 1);
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

            var nodes = tree.SearchNodes(time);

            var controlValueChange = new ControlValueChange(controlChangeEvent.ControlValue, null);

            foreach (var node in nodes)
            {
                if (node.Value.Equals(controlValueChange))
                    tree.Delete(node);
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

                    var node = tree.GetLastNodeBelowThreshold(time + 1);
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

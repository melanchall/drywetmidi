using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class PlaybackDataTracker
    {
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
            public DataChange(TData data, object metadata)
            {
                Data = data;
                Metadata = metadata;
            }

            public TData Data { get; }

            public object Metadata { get; set; }
        }

        private sealed class ProgramChange : DataChange<SevenBitNumber>
        {
            public ProgramChange(SevenBitNumber programNumber, object metadata)
                : base(programNumber, metadata)
            {
            }
        }

        private sealed class PitchValueChange : DataChange<ushort>
        {
            public PitchValueChange(ushort pitchValue, object metadata)
                : base(pitchValue, metadata)
            {
            }
        }

        private sealed class ControlValueChange : DataChange<SevenBitNumber>
        {
            public ControlValueChange(SevenBitNumber controlValue, object metadata)
                : base(controlValue, metadata)
            {
            }
        }

        #endregion

        #region Fields

        private readonly ProgramChange[] _currentProgramNumbers = new ProgramChange[FourBitNumber.MaxValue + 1];
        private readonly ValueLine<ProgramChange>[] _programLines = FourBitNumber.Values
            .Select(n => new ValueLine<ProgramChange>(new ProgramChange(SevenBitNumber.MinValue, null)))
            .ToArray();

        private readonly PitchValueChange[] _currentPitchValues = new PitchValueChange[FourBitNumber.MaxValue + 1];
        private readonly ValueLine<PitchValueChange>[] _pitchValueLines = FourBitNumber.Values
            .Select(n => new ValueLine<PitchValueChange>(new PitchValueChange(ushort.MinValue, null)))
            .ToArray();

        private readonly Dictionary<SevenBitNumber, ControlValueChange>[] _currentControlsValues = new Dictionary<SevenBitNumber, ControlValueChange>[FourBitNumber.MaxValue + 1];
        private readonly Dictionary<SevenBitNumber, ValueLine<ControlValueChange>>[] _controlsLines = FourBitNumber.Values
            .Select(n => new Dictionary<SevenBitNumber, ValueLine<ControlValueChange>>())
            .ToArray();

        private readonly TempoMap _tempoMap;

        #endregion

        #region Constructor

        public PlaybackDataTracker(TempoMap tempoMap)
        {
            _tempoMap = tempoMap;
        }

        #endregion

        #region Properties

        public bool TrackProgram { get; set; }

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

        public IEnumerable<EventWithMetadata> GetEventsAtTime(TimeSpan time)
        {
            var convertedTime = TimeConverter.ConvertFrom((MetricTimeSpan)time, _tempoMap);
            return GetControlChangeEventsAtTime(convertedTime)
                .Concat(GetProgramChangeEventsAtTime(convertedTime))
                .Concat(GetPitchBendEventsAtTime(convertedTime));
        }

        private void UpdateCurrentProgramChangeData(ProgramChangeEvent programChangeEvent, object metadata)
        {
            if (programChangeEvent == null)
                return;

            _currentProgramNumbers[programChangeEvent.Channel] = new ProgramChange(programChangeEvent.ProgramNumber, metadata);
        }

        private void InitializeProgramChangeData(ProgramChangeEvent programChangeEvent, long time, object metadata)
        {
            if (programChangeEvent == null)
                return;

            _programLines[programChangeEvent.Channel].SetValue(time, new ProgramChange(programChangeEvent.ProgramNumber, metadata));
        }

        private IEnumerable<EventWithMetadata> GetProgramChangeEventsAtTime(long time)
        {
            if (!TrackProgram)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var programChangeAtTime = _programLines[channel].GetValueAtTime(time);
                var currentProgramNumber = _currentProgramNumbers[channel];
                if (currentProgramNumber == null || !programChangeAtTime.Data.Equals(currentProgramNumber.Data))
                {
                    if (currentProgramNumber != null)
                        yield return new EventWithMetadata(
                            new ProgramChangeEvent(programChangeAtTime.Data) { Channel = channel },
                            programChangeAtTime.Metadata);
                    else
                        _currentProgramNumbers[channel] = programChangeAtTime;
                }
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

            _pitchValueLines[pitchBendEvent.Channel].SetValue(time, new PitchValueChange(pitchBendEvent.PitchValue, metadata));
        }

        private IEnumerable<EventWithMetadata> GetPitchBendEventsAtTime(long time)
        {
            if (!TrackPitchValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var pitchValueChangeAtTime = _pitchValueLines[channel].GetValueAtTime(time);
                var currentPitchValue = _currentPitchValues[channel];
                if (currentPitchValue == null || !pitchValueChangeAtTime.Data.Equals(currentPitchValue.Data))
                {
                    if (currentPitchValue != null)
                        yield return new EventWithMetadata(
                            new PitchBendEvent(pitchValueChangeAtTime.Data) { Channel = channel },
                            pitchValueChangeAtTime.Metadata);
                    else
                        _currentPitchValues[channel] = pitchValueChangeAtTime;
                }
            }
        }

        private void UpdateCurrentControlData(ControlChangeEvent controlChangeEvent, object metadata)
        {
            if (controlChangeEvent == null)
                return;

            var controlsCurrentValues = _currentControlsValues[controlChangeEvent.Channel];
            if (controlsCurrentValues == null)
                controlsCurrentValues = _currentControlsValues[controlChangeEvent.Channel] = new Dictionary<SevenBitNumber, ControlValueChange>();

            controlsCurrentValues[controlChangeEvent.ControlNumber] = new ControlValueChange(controlChangeEvent.ControlValue, metadata);
        }

        private void InitializeControlData(ControlChangeEvent controlChangeEvent, long time, object metadata)
        {
            if (controlChangeEvent == null)
                return;

            var controlsLines = _controlsLines[controlChangeEvent.Channel];

            ValueLine<ControlValueChange> controlValueLine;
            if (!controlsLines.TryGetValue(controlChangeEvent.ControlNumber, out controlValueLine))
                controlsLines.Add(controlChangeEvent.ControlNumber, controlValueLine = new ValueLine<ControlValueChange>(new ControlValueChange(SevenBitNumber.MinValue, null)));

            controlValueLine.SetValue(time, new ControlValueChange(controlChangeEvent.ControlValue, metadata));
        }

        private IEnumerable<EventWithMetadata> GetControlChangeEventsAtTime(long time)
        {
            if (!TrackControlValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var controlsLines = _controlsLines[channel];
                var controlsValues = _currentControlsValues[channel];

                foreach (var controlNumber in SevenBitNumber.Values)
                {
                    ValueLine<ControlValueChange> controlValueLine;
                    if (!controlsLines.TryGetValue(controlNumber, out controlValueLine))
                        continue;

                    var controlValueAtTime = controlValueLine.GetValueAtTime(time);

                    ControlValueChange currentControlValue = null;
                    if (controlsValues != null)
                        controlsValues.TryGetValue(controlNumber, out currentControlValue);

                    if (currentControlValue == null || !controlValueAtTime.Data.Equals(currentControlValue.Data))
                    {
                        if (currentControlValue != null)
                            yield return new EventWithMetadata(
                                new ControlChangeEvent(controlNumber, controlValueAtTime.Data) { Channel = channel },
                                controlValueAtTime.Metadata);
                        else
                        {
                            if (controlsValues == null)
                                controlsValues = _currentControlsValues[channel] = new Dictionary<SevenBitNumber, ControlValueChange>();

                            controlsValues[controlNumber] = controlValueAtTime;
                        }
                    }
                }
            }
        }

        #endregion
    }
}

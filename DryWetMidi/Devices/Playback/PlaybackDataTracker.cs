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
        #region Fields

        private readonly SevenBitNumber?[] _currentProgramNumbers = new SevenBitNumber?[FourBitNumber.MaxValue + 1];
        private readonly ValueLine<SevenBitNumber>[] _programLines = FourBitNumber.Values
            .Select(n => new ValueLine<SevenBitNumber>(SevenBitNumber.MinValue))
            .ToArray();

        private readonly ushort?[] _currentPitchValues = new ushort?[FourBitNumber.MaxValue + 1];
        private readonly ValueLine<ushort>[] _pitchValueLines = FourBitNumber.Values
            .Select(n => new ValueLine<ushort>(ushort.MinValue))
            .ToArray();

        private readonly Dictionary<SevenBitNumber, SevenBitNumber?>[] _currentControlsValues = new Dictionary<SevenBitNumber, SevenBitNumber?>[FourBitNumber.MaxValue + 1];
        private readonly Dictionary<SevenBitNumber, ValueLine<SevenBitNumber>>[] _controlsLines = FourBitNumber.Values
            .Select(n => new Dictionary<SevenBitNumber, ValueLine<SevenBitNumber>>())
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

        public void InitializeData(MidiEvent midiEvent, long time)
        {
            InitializeProgramChangeData(midiEvent as ProgramChangeEvent, time);
            InitializePitchBendData(midiEvent as PitchBendEvent, time);
            InitializeControlData(midiEvent as ControlChangeEvent, time);
        }

        public void UpdateCurrentData(MidiEvent midiEvent)
        {
            UpdateCurrentProgramChangeData(midiEvent as ProgramChangeEvent);
            UpdateCurrentPitchBendData(midiEvent as PitchBendEvent);
            UpdateCurrentControlData(midiEvent as ControlChangeEvent);
        }

        public IEnumerable<MidiEvent> GetEventsAtTime(TimeSpan time)
        {
            var convertedTime = TimeConverter.ConvertFrom((MetricTimeSpan)time, _tempoMap);
            return GetControlChangeEventsAtTime(convertedTime)
                .Concat(GetProgramChangeEventsAtTime(convertedTime))
                .Concat(GetPitchBendEventsAtTime(convertedTime));
        }

        private void UpdateCurrentProgramChangeData(ProgramChangeEvent programChangeEvent)
        {
            if (programChangeEvent == null)
                return;

            _currentProgramNumbers[programChangeEvent.Channel] = programChangeEvent.ProgramNumber;
        }

        private void InitializeProgramChangeData(ProgramChangeEvent programChangeEvent, long time)
        {
            if (programChangeEvent == null)
                return;

            _programLines[programChangeEvent.Channel].SetValue(time, programChangeEvent.ProgramNumber);
        }

        private IEnumerable<MidiEvent> GetProgramChangeEventsAtTime(long time)
        {
            if (!TrackProgram)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var programNumberAtTime = _programLines[channel].GetValueAtTime(time);
                var currentProgramNumber = _currentProgramNumbers[channel];
                if (programNumberAtTime != currentProgramNumber)
                {
                    if (currentProgramNumber != null)
                        yield return new ProgramChangeEvent(programNumberAtTime) { Channel = channel };
                    else
                        _currentProgramNumbers[channel] = programNumberAtTime;
                }
            }
        }

        private void UpdateCurrentPitchBendData(PitchBendEvent pitchBendEvent)
        {
            if (pitchBendEvent == null)
                return;

            _currentPitchValues[pitchBendEvent.Channel] = pitchBendEvent.PitchValue;
        }

        private void InitializePitchBendData(PitchBendEvent pitchBendEvent, long time)
        {
            if (pitchBendEvent == null)
                return;

            _pitchValueLines[pitchBendEvent.Channel].SetValue(time, pitchBendEvent.PitchValue);
        }

        private IEnumerable<MidiEvent> GetPitchBendEventsAtTime(long time)
        {
            if (!TrackPitchValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var pitchValueAtTime = _pitchValueLines[channel].GetValueAtTime(time);
                var currentPitchValue = _currentPitchValues[channel];
                if (pitchValueAtTime != currentPitchValue)
                {
                    if (currentPitchValue != null)
                        yield return new PitchBendEvent(pitchValueAtTime) { Channel = channel };
                    else
                        _currentPitchValues[channel] = pitchValueAtTime;
                }
            }
        }

        private void UpdateCurrentControlData(ControlChangeEvent controlChangeEvent)
        {
            if (controlChangeEvent == null)
                return;

            var controlsCurrentValues = _currentControlsValues[controlChangeEvent.Channel];
            if (controlsCurrentValues == null)
                controlsCurrentValues = _currentControlsValues[controlChangeEvent.Channel] = new Dictionary<SevenBitNumber, SevenBitNumber?>();

            controlsCurrentValues[controlChangeEvent.ControlNumber] = controlChangeEvent.ControlValue;
        }

        private void InitializeControlData(ControlChangeEvent controlChangeEvent, long time)
        {
            if (controlChangeEvent == null)
                return;

            var controlsLines = _controlsLines[controlChangeEvent.Channel];

            ValueLine<SevenBitNumber> controlValueLine;
            if (!controlsLines.TryGetValue(controlChangeEvent.ControlNumber, out controlValueLine))
                controlsLines.Add(controlChangeEvent.ControlNumber, controlValueLine = new ValueLine<SevenBitNumber>(SevenBitNumber.MinValue));

            controlValueLine.SetValue(time, controlChangeEvent.ControlValue);
        }

        private IEnumerable<MidiEvent> GetControlChangeEventsAtTime(long time)
        {
            if (!TrackControlValue)
                yield break;

            foreach (var channel in FourBitNumber.Values)
            {
                var controlsLines = _controlsLines[channel];
                var controlsValues = _currentControlsValues[channel];

                foreach (var controlNumber in SevenBitNumber.Values)
                {
                    ValueLine<SevenBitNumber> controlValueLine;
                    if (!controlsLines.TryGetValue(controlNumber, out controlValueLine))
                        continue;

                    var controlValueAtTime = controlValueLine.GetValueAtTime(time);

                    SevenBitNumber? currentControlValue = null;
                    if (controlsValues != null)
                        controlsValues.TryGetValue(controlNumber, out currentControlValue);

                    if (controlValueAtTime != currentControlValue)
                    {
                        if (currentControlValue != null)
                            yield return new ControlChangeEvent(controlNumber, controlValueAtTime) { Channel = channel };
                        else
                        {
                            if (controlsValues == null)
                                controlsValues = _currentControlsValues[channel] = new Dictionary<SevenBitNumber, SevenBitNumber?>();

                            controlsValues[controlNumber] = controlValueAtTime;
                        }
                    }
                }
            }
        }

        #endregion
    }
}

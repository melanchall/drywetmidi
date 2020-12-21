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

        #endregion

        #region Methods

        public void InitializeData(MidiEvent midiEvent, long time)
        {
            InitializeProgramChangeData(midiEvent as ProgramChangeEvent, time);
        }

        public void UpdateCurrentData(MidiEvent midiEvent)
        {
            UpdateCurrentProgramChangeData(midiEvent as ProgramChangeEvent);
        }

        public IEnumerable<MidiEvent> GetEventsAtTime(TimeSpan time)
        {
            var convertedTime = TimeConverter.ConvertFrom((MetricTimeSpan)time, _tempoMap);
            return GetProgramChangeEventsAtTime(convertedTime);
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

        #endregion
    }
}

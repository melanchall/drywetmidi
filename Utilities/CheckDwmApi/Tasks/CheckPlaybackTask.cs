using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Melanchall.CheckDwmApi
{
    internal sealed class CheckPlaybackTask : ITask
    {
        private record EventInfo(MidiEvent Event, long TimeMs);

        private readonly MidiFile _midiFile;
        private readonly bool _needRegularPrecisionTickGenerator;

        public CheckPlaybackTask(
            MidiFile midiFile,
            bool needRegularPrecisionTickGenerator)
        {
            _midiFile = midiFile;
            _needRegularPrecisionTickGenerator = needRegularPrecisionTickGenerator;
        }

        public string GetTitle() =>
            "Check playback functionality";

        public string GetDescription() => @"
The tool will check if playback functionality is working correctly.";

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            var expectedEvents = GetExpectedEvents(reportWriter);

            var playedEvents = new List<EventInfo>();
            var stopwatch = new Stopwatch();

            var playback = CreatePlayback(playedEvents, stopwatch, reportWriter);
            StartPlayback(playback, stopwatch, reportWriter);

            WaitingPlaybackFinished(playback, reportWriter);

            if (playedEvents.Count != expectedEvents.Length)
                throw new TaskFailedException($"Number of played events ({playedEvents.Count}) does not match the expected one ({expectedEvents.Length}).");

            WritePlayedEventsInfo(playedEvents, expectedEvents, reportWriter);
        }

        private void WritePlayedEventsInfo(
            List<EventInfo> playedEvents,
            EventInfo[] expectedEvents,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle("Writing played events info...");

            var midiEventEqualityCheckSettings = new MidiEventEqualityCheckSettings
            {
                CompareDeltaTimes = false,
            };

            for (int i = 0; i < expectedEvents.Length; i++)
            {
                var (expectedEvent, expectedTime) = expectedEvents[i];
                var (playedEvent, playedTime) = playedEvents[i];

                reportWriter.WriteOperationSubTitle($"{expectedTime} -> {playedTime}: {expectedEvent}{(MidiEvent.Equals(expectedEvent, playedEvent, midiEventEqualityCheckSettings) ? string.Empty : $" -> {playedEvent}")}");
            }
        }

        private EventInfo[] GetExpectedEvents(
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle("Getting expected events...");
            var tempoMap = _midiFile.GetTempoMap();
            var result = _midiFile
                .GetTimedEvents()
                .Select(e => new EventInfo(e.Event, (long)e.TimeAs<MetricTimeSpan>(tempoMap).TotalMilliseconds))
                .ToArray();
            reportWriter.WriteOperationSubTitle("done");
            return result;
        }

        private void WaitingPlaybackFinished(
            Playback playback,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle("Waiting for playback finished...");

            var timeout = TimeSpan.FromSeconds(10);
            var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
            if (!playbackStopped)
                throw new TaskFailedException("Playback didn't stop within the timeout period.");

            reportWriter.WriteOperationSubTitle("playback finished");
        }

        private Playback CreatePlayback(
            List<EventInfo> playedEvents,
            Stopwatch stopwatch,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle("Creating playback...");

            PlaybackSettings playbackSettings = null;
            if (_needRegularPrecisionTickGenerator)
            {
                reportWriter.WriteOperationSubTitle("regular precision tick generator will be used");
                playbackSettings = new PlaybackSettings
                {
                    ClockSettings = new MidiClockSettings
                    {
                        CreateTickGeneratorCallback = () => new RegularPrecisionTickGenerator(),
                    },
                };
            }

            var playback = _midiFile.GetPlayback(playbackSettings);
            
            reportWriter.WriteOperationSubTitle("created");

            reportWriter.WriteOperationTitle("Subscribing to EventPlayed event...");
            playback.EventPlayed += (_, e) =>
            {
                reportWriter.WriteEventInfo($"played event: '{e.Event}'");
                playedEvents.Add(new (e.Event, stopwatch.ElapsedMilliseconds));
            };
            reportWriter.WriteOperationSubTitle("subscribed");

            return playback;
        }

        private void StartPlayback(
            Playback playback,
            Stopwatch stopwatch,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle("Starting playback...");
            playback.Start();
            reportWriter.WriteOperationSubTitle("started");

            stopwatch.Start();
        }
    }
}

using System;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Diagnostics;
using Melanchall.DryWetMidi.Configuration;

namespace DwmNetConsoleApp
{
    class Program
    {
        private const string WmsActiveEnvironmentVariableName = "WMS_ACTIVE";

        static void Main(string[] args)
        {
            Console.WriteLine($"OS version: {Environment.OSVersion}");
            Console.WriteLine($"CLR version: {Environment.Version}");
            Console.WriteLine($"Library configuration summary:{Environment.NewLine}{LibraryConfiguration.GetConfigurationSummary()}");
            Console.WriteLine("---------------------------------");

            Console.WriteLine("Playing MIDI data...");

            var tempoMap = TempoMap.Default;

            var eventsToPlay = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100)),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0))
                    .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), tempoMap)
            };

            var playedEvents = new List<MidiEvent>();
            var stopwatch = new Stopwatch();

            using var outputEndpoint = OutputEndpoint.GetByName("MIDI A");
            using var inputEndpoint = InputEndpoint.GetByName("MIDI A");
            using var playback = new Playback(eventsToPlay, TempoMap.Default, outputEndpoint);

            inputEndpoint.EventReceived += (_, e) =>
            {
                Console.WriteLine($"[{stopwatch.ElapsedMilliseconds} ms] Event received: {e.Event}");
            };
            inputEndpoint.StartEventsListening();

            playback.EventPlayed += (_, e) =>
            {
                Console.WriteLine($"[{stopwatch.ElapsedMilliseconds} ms] Event played: {e.Event}");
                playedEvents.Add(e.Event);
            };

            playback.Start();
            stopwatch.Start();

            var timeout = TimeSpan.FromSeconds(10);
            var ok = SpinWait.SpinUntil(
                () => !playback.IsRunning && playedEvents.Count == 2,
                timeout);

            if (!ok)
                throw new InvalidOperationException($"Playback was not completed within {timeout}.");

            if (IsWmsActive())
            {
                Console.WriteLine("WMS is active. Checking Virtual devices API...");
                var virtualDevice = VirtualDevice.Create(Guid.NewGuid().ToString().Trim('{', '}'));
                Console.WriteLine($"Virtual device created: {virtualDevice.Name}.");
            }

            Console.WriteLine($"[{stopwatch.ElapsedMilliseconds} ms] Played.");
        }

        private static bool IsWmsActive()
        {
            var value = Environment.GetEnvironmentVariable(WmsActiveEnvironmentVariableName);
            return
                string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "1", StringComparison.Ordinal);
        }
    }
}

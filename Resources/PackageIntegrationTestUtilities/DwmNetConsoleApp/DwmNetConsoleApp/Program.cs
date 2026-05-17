using System;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Diagnostics;

namespace DwmNetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"OS version: {Environment.OSVersion}");
            Console.WriteLine($"CLR version: {Environment.Version}");
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

            Console.WriteLine($"[{stopwatch.ElapsedMilliseconds} ms] Played.");
        }
    }
}

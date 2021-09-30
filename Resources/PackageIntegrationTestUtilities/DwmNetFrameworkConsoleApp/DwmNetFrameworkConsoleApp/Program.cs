using System;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;

namespace DwmNetFrameworkConsoleApp
{
    class Program
    {
        private static int _playedEventsCount = 0;

        static void Main(string[] args)
        {
            Console.WriteLine($"Is 64-bit operating system: {Environment.Is64BitOperatingSystem}");
            Console.WriteLine($"Is 64-bit process: {Environment.Is64BitProcess}");
            Console.WriteLine($"OS version: {Environment.OSVersion}");
            Console.WriteLine($"CLR version: {Environment.Version}");
            Console.WriteLine("---------------------------------");

            Console.WriteLine("Playing MIDI data...");

            var eventsToPlay = new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0) { DeltaTime = 100 }
            };

            using (var outputDevice = OutputDevice.GetByName("MIDI A"))
            using (var playback = eventsToPlay.GetPlayback(TempoMap.Default, outputDevice))
            {
                playback.EventPlayed += OnEventPlayed;
                playback.Start();

                SpinWait.SpinUntil(() => !playback.IsRunning && _playedEventsCount == 2);
            }

            Console.WriteLine("Played.");
        }

        private static void OnEventPlayed(object sender, MidiEventPlayedEventArgs e)
        {
            Console.WriteLine($"Event played: {e.Event}");
            _playedEventsCount++;
        }
    }
}

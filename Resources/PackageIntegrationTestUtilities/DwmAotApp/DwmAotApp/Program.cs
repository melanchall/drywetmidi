using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DwmAotApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CheckAddTextEventAction();
            CheckCustomChunk();
            CheckCustomMetaEvent();
            CheckOctave();
            CheckCsvSerializer();
            CheckPlayback();

            Console.WriteLine("All done.");
        }

        private static void CheckAddTextEventAction()
        {
            Console.WriteLine("Checking AddTextEventAction...");

            var pattern = new PatternBuilder()
                .Marker("A")
                .Lyrics("B")
                .Build();

            var midiFile = WriteRead(pattern.ToFile(TempoMap.Default));

            var textEvents = midiFile
                .GetTrackChunks()
                .SelectMany(c => c.Events)
                .Where(e => e is BaseTextEvent)
                .ToArray();

            var expectedTextEvents = new MidiEvent[]
            {
                new MarkerEvent("A"),
                new LyricEvent("B"),
            };

            var success =
                textEvents.Length == expectedTextEvents.Length &&
                Enumerable.Range(0, textEvents.Length).All(i => MidiEvent.Equals(textEvents[i], expectedTextEvents[i]));

            if (!success)
                throw new InvalidOperationException("Check failed.");

            Console.WriteLine("AddTextEventAction check passed.");
        }

        private static void CheckCustomChunk()
        {
            Console.WriteLine("Checking CustomChunk...");

            var midiFile = WriteRead(
                new MidiFile(new CustomChunk { X = 42 }),
                readingSettings: new ReadingSettings
                {
                    CustomChunkTypes = new ChunkTypesCollection
                    {
                        { typeof(CustomChunk), "cust" },
                    }
                });

            var customChunk = midiFile.Chunks.OfType<CustomChunk>().FirstOrDefault();

            var success =
                customChunk != null &&
                customChunk.X == 42;

            if (!success)
                throw new InvalidOperationException("Check failed.");

            Console.WriteLine("CustomChunk check passed.");
        }

        private static void CheckCustomMetaEvent()
        {
            Console.WriteLine("Checking CustomMetaEvent...");

            var midiFile = WriteRead(
                new MidiFile(new TrackChunk(new CustomMetaEvent { X = 42 })),
                writingSettings: new WritingSettings
                {
                    CustomMetaEventTypes = new EventTypesCollection
                    {
                        { typeof(CustomMetaEvent), 0xAF },
                    }
                },
                readingSettings: new ReadingSettings
                {
                    CustomMetaEventTypes = new EventTypesCollection
                    {
                        { typeof(CustomMetaEvent), 0xAF },
                    }
                });

            var customMetaEvent = midiFile
                .GetTrackChunks()
                .SelectMany(c => c.Events)
                .OfType<CustomMetaEvent>()
                .FirstOrDefault();

            var success =
                customMetaEvent != null &&
                customMetaEvent.X == 42;

            if (!success)
                throw new InvalidOperationException("Check failed.");

            Console.WriteLine("CustomMetaEvent check passed.");
        }

        private static void CheckOctave()
        {
            Console.WriteLine("Checking Octave...");

            var octave = Octave.Get(4);

            var success =
                octave.C.NoteName == NoteName.C &&
                octave.CSharp.NoteName == NoteName.CSharp &&
                octave.D.NoteName == NoteName.D &&
                octave.DSharp.NoteName == NoteName.DSharp &&
                octave.E.NoteName == NoteName.E &&
                octave.F.NoteName == NoteName.F &&
                octave.FSharp.NoteName == NoteName.FSharp &&
                octave.G.NoteName == NoteName.G &&
                octave.GSharp.NoteName == NoteName.GSharp &&
                octave.A.NoteName == NoteName.A &&
                octave.ASharp.NoteName == NoteName.ASharp &&
                octave.B.NoteName == NoteName.B;

            if (!success)
                throw new InvalidOperationException("Check failed.");

            Console.WriteLine("Octave check passed.");
        }

        private static void CheckCsvSerializer()
        {
            Console.WriteLine("Checking CsvSerializer...");

            var tempFilePath = Path.GetTempFileName();
            File.WriteAllLines(tempFilePath, new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0/1,\"A\"",
                $"1,\"MTrk\",0,\"Text\",1/4,\"B\"",
                $"2,\"MTrk\",0,\"NoteOn\",0/1,4,100,127",
                $"2,\"MTrk\",0,\"NoteOff\",1/4,4,100,0",
            });

            var expectedMidiFile = new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }));

            var midiFile = CsvSerializer.DeserializeFileFromCsv(tempFilePath);

            var success = MidiFile.Equals(midiFile, expectedMidiFile);
            if (!success)
                throw new InvalidOperationException("Check failed.");

            Console.WriteLine("CsvSerializer check passed.");
            File.Delete(tempFilePath);
        }

        private static void CheckPlayback()
        {
            Console.WriteLine("Checking Playback...");

            var tempoMap = TempoMap.Default;

            var eventsToPlay = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100)),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0))
                    .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), tempoMap)
            };

            var playedEvents = new List<MidiEvent>();
            var stopwatch = new Stopwatch();

            using var outputDevice = OutputDevice.GetByName("MIDI A");
            using var inputDevice = InputDevice.GetByName("MIDI A");
            using var playback = new Playback(eventsToPlay, TempoMap.Default, outputDevice);

            inputDevice.EventReceived += (_, e) =>
            {
                Console.WriteLine($"[{stopwatch.ElapsedMilliseconds} ms] Event received: {e.Event}");
            };
            inputDevice.StartEventsListening();

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

        private static MidiFile WriteRead(
            MidiFile midiFile,
            WritingSettings writingSettings = null,
            ReadingSettings readingSettings = null)
        {
            var tempFilePath = Path.GetTempFileName();

            try
            {
                midiFile.Write(tempFilePath, true, settings: writingSettings);
                return MidiFile.Read(tempFilePath, readingSettings);
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}

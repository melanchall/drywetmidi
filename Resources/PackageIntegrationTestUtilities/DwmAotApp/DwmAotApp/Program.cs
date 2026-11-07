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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InteractionNote = Melanchall.DryWetMidi.Interaction.Note;
using MusicTheoryNote = Melanchall.DryWetMidi.MusicTheory.Note;

namespace DwmAotApp
{
    internal class Program
    {
        private const string WmsActiveEnvironmentVariableName = "WMS_ACTIVE";

        static async Task Main(string[] args)
        {
            CheckAddTextEventAction();
            CheckCustomChunk();
            CheckCustomMetaEvent();
            CheckOctave();
            CheckCsvSerializer();
            await CheckLazyIoAsync();
            CheckJsonSerialization();
            await CheckCsvRoundTripsAsync();
            CheckParsing();

            if (args.Contains("--no-multimedia"))
            {
                Console.WriteLine("Multimedia checks explicitly skipped.");
            }
            else
            {
                CheckPlayback();

                if (IsWmsActive())
                {
                    Console.WriteLine("WMS is active. Checking Virtual devices API...");
                    CheckVirtualDevice();
                }
            }

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

        private static MidiFile WriteRead(
            MidiFile midiFile,
            WritingSettings writingSettings = null,
            ReadingSettings readingSettings = null)
        {
            var tempFilePath = Path.GetTempFileName();
            var asynchronousFilePath = Path.GetTempFileName();
            var asynchronousInputFile = midiFile.Clone();

            try
            {
                midiFile.Write(tempFilePath, true, settings: writingSettings);
                var synchronousFile = MidiFile.Read(tempFilePath, readingSettings);

                asynchronousInputFile.WriteAsync(
                    asynchronousFilePath,
                    true,
                    settings: writingSettings,
                    cancellationToken: CancellationToken.None).Wait();
                var asynchronousFile = MidiFile.ReadAsync(
                    asynchronousFilePath,
                    readingSettings,
                    CancellationToken.None).Result;

                if (!MidiFile.Equals(synchronousFile, asynchronousFile))
                    throw new InvalidOperationException("Synchronous and asynchronous MIDI file round-trips differ.");

                return synchronousFile;
            }
            finally
            {
                File.Delete(tempFilePath);
                File.Delete(asynchronousFilePath);
            }
        }

        private static void CheckVirtualDevice()
        {
            var virtualDevice = VirtualDevice.Create(Guid.NewGuid().ToString().Trim('{', '}'));
            Console.WriteLine($"Virtual device created: {virtualDevice.Name}.");
        }

        private static bool IsWmsActive()
        {
            var value = Environment.GetEnvironmentVariable(WmsActiveEnvironmentVariableName);
            return
                string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "1", StringComparison.Ordinal);
        }

        private static async Task CheckLazyIoAsync()
        {
            Console.WriteLine("Checking synchronous and asynchronous lazy MIDI I/O...");

            foreach (var useAsync in new[] { false, true })
            {
                var expectedFile = CreateSampleFile();
                using var stream = new MemoryStream();

                if (useAsync)
                {
                    await using (var writer = await MidiFile.WriteLazyAsync(
                        stream,
                        cancellationToken: CancellationToken.None))
                    {
                        foreach (var trackChunk in expectedFile.GetTrackChunks())
                        {
                            await writer.StartTrackChunkAsync(CancellationToken.None);

                            foreach (var midiEvent in trackChunk.Events)
                            {
                                await writer.WriteEventAsync(midiEvent.Clone(), CancellationToken.None);
                            }

                            await writer.EndTrackChunkAsync(CancellationToken.None);
                        }
                    }
                }
                else
                {
                    using (var writer = MidiFile.WriteLazy(stream))
                    {
                        foreach (var trackChunk in expectedFile.GetTrackChunks())
                        {
                            writer.StartTrackChunk();

                            foreach (var midiEvent in trackChunk.Events)
                            {
                                writer.WriteEvent(midiEvent.Clone());
                            }

                            writer.EndTrackChunk();
                        }
                    }
                }

                stream.Position = 0;
                var writtenFile = MidiFile.Read(stream);

                CheckMidiFileEquality(expectedFile, writtenFile, $"Lazy writing (async: {useAsync})");

                stream.Position = 0;
                var tokens = new List<MidiToken>();

                if (useAsync)
                {
                    await using var reader = await MidiFile.ReadLazyAsync(
                        stream,
                        cancellationToken: CancellationToken.None);

                    await foreach (var token in reader.EnumerateTokensAsync(CancellationToken.None))
                    {
                        tokens.Add(token);
                    }
                }
                else
                {
                    using var reader = MidiFile.ReadLazy(stream);
                    tokens.AddRange(reader.EnumerateTokens());
                }

                var fileHeader = tokens.OfType<FileHeaderToken>().Single();
                var eventTokens = tokens.OfType<MidiEventToken>()
                    .Where(token => !(token.Event is EndOfTrackEvent))
                    .ToArray();
                var expectedEvents = writtenFile.GetTrackChunks().SelectMany(chunk => chunk.Events).ToArray();

                if (fileHeader.TracksNumber != 2 ||
                    fileHeader.FileFormat != (ushort)MidiFileFormat.MultiTrack ||
                    !fileHeader.TimeDivision.Equals(expectedFile.TimeDivision) ||
                    tokens.OfType<ChunkHeaderToken>().Count() != 3 ||
                    tokens.OfType<MidiEventToken>().Count(token => token.Event is EndOfTrackEvent) != 2 ||
                    eventTokens.Length != expectedEvents.Length ||
                    !eventTokens.Select(token => token.Event).Zip(expectedEvents, MidiEvent.Equals).All(isEqual => isEqual))
                    throw new InvalidOperationException($"Lazy reading returned incorrect tokens (async: {useAsync}).");
            }

            Console.WriteLine("Lazy MIDI I/O checks passed.");
        }

        private static void CheckJsonSerialization()
        {
            Console.WriteLine("Checking source-generated JSON serialization...");

            var fourBitNumber = FourBitNumber.MaxValue;
            var sevenBitNumber = SevenBitNumber.MaxValue;
            var fourBitJson = JsonSerializer.Serialize(fourBitNumber, AotJsonContext.Default.FourBitNumber);
            var sevenBitJson = JsonSerializer.Serialize(sevenBitNumber, AotJsonContext.Default.SevenBitNumber);

            if (fourBitJson != "15" ||
                sevenBitJson != "127" ||
                JsonSerializer.Deserialize(fourBitJson, AotJsonContext.Default.FourBitNumber) != fourBitNumber ||
                JsonSerializer.Deserialize(sevenBitJson, AotJsonContext.Default.SevenBitNumber) != sevenBitNumber)
                throw new InvalidOperationException("Bit-number JSON values did not round-trip as numbers.");

            var fourBitDictionary = new Dictionary<FourBitNumber, SevenBitNumber>
            {
                [FourBitNumber.MinValue] = SevenBitNumber.MinValue,
                [fourBitNumber] = sevenBitNumber
            };
            var sevenBitDictionary = new Dictionary<SevenBitNumber, FourBitNumber>
            {
                [SevenBitNumber.MinValue] = FourBitNumber.MinValue,
                [sevenBitNumber] = fourBitNumber
            };

            var fourBitDictionaryJson = JsonSerializer.Serialize(
                fourBitDictionary,
                AotJsonContext.Default.FourBitNumberDictionary);
            var sevenBitDictionaryJson = JsonSerializer.Serialize(
                sevenBitDictionary,
                AotJsonContext.Default.SevenBitNumberDictionary);
            var restoredFourBitDictionary = JsonSerializer.Deserialize(
                fourBitDictionaryJson,
                AotJsonContext.Default.FourBitNumberDictionary);
            var restoredSevenBitDictionary = JsonSerializer.Deserialize(
                sevenBitDictionaryJson,
                AotJsonContext.Default.SevenBitNumberDictionary);

            if (fourBitDictionaryJson != "{\"0\":0,\"15\":127}" ||
                sevenBitDictionaryJson != "{\"0\":0,\"127\":15}" ||
                restoredFourBitDictionary == null ||
                restoredSevenBitDictionary == null ||
                restoredFourBitDictionary.Count != 2 ||
                restoredSevenBitDictionary.Count != 2 ||
                !fourBitDictionary.All(pair => restoredFourBitDictionary.TryGetValue(pair.Key, out var value) && value == pair.Value) ||
                !sevenBitDictionary.All(pair => restoredSevenBitDictionary.TryGetValue(pair.Key, out var value) && value == pair.Value))
                throw new InvalidOperationException("Bit-number JSON dictionary keys and values did not round-trip.");

            ExpectJsonException(
                () => JsonSerializer.Deserialize("16", AotJsonContext.Default.FourBitNumber),
                "Out-of-range FourBitNumber");
            ExpectJsonException(
                () => JsonSerializer.Deserialize("128", AotJsonContext.Default.SevenBitNumber),
                "Out-of-range SevenBitNumber");
            ExpectJsonException(
                () => JsonSerializer.Deserialize("{\"16\":0}", AotJsonContext.Default.FourBitNumberDictionary),
                "Out-of-range FourBitNumber key");
            ExpectJsonException(
                () => JsonSerializer.Deserialize("{\"128\":0}", AotJsonContext.Default.SevenBitNumberDictionary),
                "Out-of-range SevenBitNumber key");

            Console.WriteLine("JSON serialization checks passed.");
        }

        private static async Task CheckCsvRoundTripsAsync()
        {
            Console.WriteLine("Checking CSV stream round-trips for files, chunks, and timed objects...");

            foreach (var useAsync in new[] { false, true })
            {
                var expectedFile = CreateSampleFile();
                using var fileStream = new MemoryStream();

                if (useAsync)
                    await expectedFile.SerializeToCsvAsync(fileStream, cancellationToken: CancellationToken.None);
                else
                    expectedFile.SerializeToCsv(fileStream);

                fileStream.Position = 0;
                var restoredFile = useAsync
                    ? await CsvSerializer.DeserializeFileFromCsvAsync(fileStream, cancellationToken: CancellationToken.None)
                    : CsvSerializer.DeserializeFileFromCsv(fileStream);

                CheckMidiFileEquality(expectedFile, restoredFile, $"CSV file stream round-trip (async: {useAsync})");

                var expectedChunk = expectedFile.GetTrackChunks().Last().Clone();
                using var chunkStream = new MemoryStream();

                if (useAsync)
                {
                    await expectedChunk.SerializeToCsvAsync(
                        chunkStream,
                        TempoMap.Default,
                        cancellationToken: CancellationToken.None);
                }
                else
                    expectedChunk.SerializeToCsv(chunkStream, TempoMap.Default);

                chunkStream.Position = 0;
                var restoredChunk = useAsync
                    ? await CsvSerializer.DeserializeChunkFromCsvAsync(
                        chunkStream,
                        TempoMap.Default,
                        cancellationToken: CancellationToken.None)
                    : CsvSerializer.DeserializeChunkFromCsv(chunkStream, TempoMap.Default);

                if (!MidiChunk.Equals(expectedChunk, restoredChunk))
                    throw new InvalidOperationException($"CSV chunk stream round-trip changed MIDI data (async: {useAsync}).");

                var expectedObjects = new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A,\"B\"\nC"), 120),
                    new InteractionNote((SevenBitNumber)60, 480, 240)
                    {
                        Channel = (FourBitNumber)4,
                        Velocity = (SevenBitNumber)100,
                        OffVelocity = (SevenBitNumber)20
                    }
                };
                var serializationSettings = new CsvSerializationSettings
                {
                    TimeType = TimeSpanType.Musical,
                    LengthType = TimeSpanType.Musical
                };
                using var objectsStream = new MemoryStream();

                if (useAsync)
                {
                    await expectedObjects.SerializeToCsvAsync(
                        objectsStream,
                        TempoMap.Default,
                        serializationSettings,
                        CancellationToken.None);
                }
                else
                    expectedObjects.SerializeToCsv(objectsStream, TempoMap.Default, serializationSettings);

                objectsStream.Position = 0;
                var restoredObjects = useAsync
                    ? await CsvSerializer.DeserializeObjectsFromCsvAsync(
                        objectsStream,
                        TempoMap.Default,
                        cancellationToken: CancellationToken.None)
                    : CsvSerializer.DeserializeObjectsFromCsv(objectsStream, TempoMap.Default);

                if (restoredObjects.Count != 2 ||
                    restoredObjects.OfType<InteractionNote>().Count() != 1 ||
                    restoredObjects.OfType<TimedEvent>().Count() != 1 ||
                    !MidiFile.Equals(expectedObjects.ToFile(), restoredObjects.ToFile()))
                    throw new InvalidOperationException($"CSV timed-object stream round-trip changed objects (async: {useAsync}).");
            }

            Console.WriteLine("CSV stream round-trip checks passed.");
        }

        private static void CheckParsing()
        {
            Console.WriteLine("Checking time-span and music-theory parsing...");

            var metricTimeSpan = MetricTimeSpan.Parse("1h2m3s4ms");
            var musicalTimeSpan = MusicalTimeSpan.Parse("3/8");
            var musicTheoryNote = MusicTheoryNote.Parse("C#4");

            if (!metricTimeSpan.Equals(new MetricTimeSpan(1, 2, 3, 4)) ||
                !musicalTimeSpan.Equals(new MusicalTimeSpan(3, 8)) ||
                musicTheoryNote.NoteNumber != (SevenBitNumber)61 ||
                !MetricTimeSpan.TryParse(metricTimeSpan.ToString(), out var restoredMetricTimeSpan) ||
                !metricTimeSpan.Equals(restoredMetricTimeSpan) ||
                !MusicalTimeSpan.TryParse(musicalTimeSpan.ToString(), out var restoredMusicalTimeSpan) ||
                !musicalTimeSpan.Equals(restoredMusicalTimeSpan) ||
                !MusicTheoryNote.TryParse(musicTheoryNote.ToString(), out var restoredNote) ||
                restoredNote.NoteNumber != musicTheoryNote.NoteNumber)
                throw new InvalidOperationException("Parsing or formatting time spans and notes returned incorrect values.");

            if (MetricTimeSpan.TryParse("invalid", out _) ||
                MusicalTimeSpan.TryParse("invalid", out _) ||
                MusicTheoryNote.TryParse("invalid", out _))
                throw new InvalidOperationException("A parser accepted invalid input.");

            Console.WriteLine("Parsing checks passed.");
        }

        private static MidiFile CreateSampleFile()
        {
            return new MidiFile(
                new TrackChunk(
                    new TextEvent("A,\"B\"\nC"),
                    new SetTempoEvent(400000)),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = 120
                    },
                    new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)20)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = 480
                    }));
        }

        private static void CheckMidiFileEquality(MidiFile expectedFile, MidiFile actualFile, string scenario)
        {
            var equalitySettings = new MidiFileEqualityCheckSettings
            {
                CompareOriginalFormat = false
            };

            if (!MidiFile.Equals(expectedFile, actualFile, equalitySettings, out var message))
                throw new InvalidOperationException($"{scenario} changed MIDI data: {message}");
        }

        private static void ExpectJsonException(Action action, string scenario)
        {
            try
            {
                action();
            }
            catch (JsonException)
            {
                return;
            }

            throw new InvalidOperationException($"{scenario} JSON was accepted.");
        }
    }
}

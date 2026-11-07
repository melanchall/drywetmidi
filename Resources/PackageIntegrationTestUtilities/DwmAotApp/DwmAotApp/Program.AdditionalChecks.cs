using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InteractionNote = Melanchall.DryWetMidi.Interaction.Note;
using MusicTheoryNote = Melanchall.DryWetMidi.MusicTheory.Note;

namespace DwmAotApp
{
    internal partial class Program
    {
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

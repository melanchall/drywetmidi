using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiTokensReaderUtilitiesTests
    {
        [Test]
        public async Task EnumerateEventsAsync_ConsecutiveTracks([Values] bool nonSeekable)
        {
            var file = new MidiFile(
                new TrackChunk(new TextEvent("first")),
                new TrackChunk(new TextEvent("second")));
            using var stream = new AsyncOnlyStream(GetBytes(file), !nonSeekable, 1);
            await using var reader = await MidiFile.ReadLazyAsync(stream);

            await MoveToTrackAsync(reader);
            var firstResult = reader.EnumerateEventsAsync();
            var firstEvents = await CollectAsync(firstResult.Events);

            ClassicAssert.IsTrue(firstResult.IsCompleted, "First track enumeration did not complete.");
            ClassicAssert.AreEqual("first", firstEvents.OfType<TextEvent>().Single().Text, "First track events changed.");
            ClassicAssert.IsInstanceOf<ChunkHeaderToken>(firstResult.NextToken, "The next track header was not preserved.");
            ClassicAssert.AreEqual(TrackChunk.Id, ((ChunkHeaderToken)firstResult.NextToken).ChunkId, "The preserved token is not a track header.");

            var secondResult = reader.EnumerateEventsAsync();
            var secondEvents = await CollectAsync(secondResult.Events);

            ClassicAssert.IsTrue(secondResult.IsCompleted, "Second track enumeration did not complete.");
            ClassicAssert.IsNull(secondResult.NextToken, "EOF produced a boundary token.");
            ClassicAssert.AreEqual("second", secondEvents.OfType<TextEvent>().Single().Text, "Second track events changed.");
            ClassicAssert.IsFalse(stream.IsDisposed, "Enumeration disposed the caller-owned stream.");
        }

        [Test]
        public async Task EnumerateEventsAsync_NonEventBoundary()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile(new TrackChunk())), false, 1);
            await using var reader = await MidiFile.ReadLazyAsync(stream);
            var result = reader.EnumerateEventsAsync();

            var events = await CollectAsync(result.Events);

            ClassicAssert.AreEqual(0, events.Count, "A non-event token was returned as an event.");
            ClassicAssert.IsTrue(result.IsCompleted, "An immediate boundary did not complete enumeration.");
            ClassicAssert.IsInstanceOf<ChunkHeaderToken>(result.NextToken, "The first chunk header was lost.");
            ClassicAssert.IsInstanceOf<FileHeaderToken>(await reader.ReadTokenAsync(), "Enumeration read past the boundary token.");
        }

        [Test]
        public async Task EnumerateEventsAsync_EarlyDisposalDoesNotReadAhead()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile(
                new TrackChunk(new TextEvent("first"), new TextEvent("second")))), false, 1);
            await using var reader = await MidiFile.ReadLazyAsync(stream);
            await MoveToTrackAsync(reader);
            var readsBeforeCreation = stream.AsyncReadsCount;
            var result = reader.EnumerateEventsAsync();

            ClassicAssert.AreEqual(readsBeforeCreation, stream.AsyncReadsCount, "Creating the result performed I/O.");
            ClassicAssert.IsFalse(result.IsCompleted, "An unstarted sequence was marked completed.");

            await using (var enumerator = result.Events.GetAsyncEnumerator())
            {
                ClassicAssert.IsTrue(await enumerator.MoveNextAsync(), "The first event was not returned.");
                ClassicAssert.IsFalse(result.IsCompleted, "Yielding an event completed enumeration.");
            }

            ClassicAssert.IsFalse(result.IsCompleted, "Disposing early completed enumeration.");
            ClassicAssert.IsNull(result.NextToken, "Early disposal read a boundary token.");
            var nextToken = (MidiEventToken)await reader.ReadTokenAsync();
            ClassicAssert.AreEqual("second", ((TextEvent)nextToken.Event).Text, "Early disposal drained the remaining events.");
        }

        [Test]
        public async Task EnumerateEventsAsync_RejectsRepeatedEnumeration()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile()));
            await using var reader = await MidiFile.ReadLazyAsync(stream);
            var result = reader.EnumerateEventsAsync();
            await using var enumerator = result.Events.GetAsyncEnumerator();

            ClassicAssert.Throws<InvalidOperationException>(
                () => result.Events.GetAsyncEnumerator(),
                "The sequence accepted a second enumerator.");
        }

        [Test]
        public async Task EnumerateEventsAsync_CancellationInterruptsPendingRead()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile(
                new TrackChunk(new TextEvent("event")))), false, 1);
            await using var reader = await MidiFile.ReadLazyAsync(stream);
            await MoveToTrackAsync(reader);
            stream.ReadGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var cancellation = new CancellationTokenSource();
            var result = reader.EnumerateEventsAsync();
            await using var enumerator = result.Events.GetAsyncEnumerator(cancellation.Token);

            var pendingMove = enumerator.MoveNextAsync().AsTask();
            ClassicAssert.IsFalse(pendingMove.IsCompleted, "Enumeration did not await stream input.");
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(
                async () => await pendingMove.WaitAsync(TimeSpan.FromSeconds(10)),
                "Cancellation did not interrupt the pending read.");
            ClassicAssert.IsFalse(result.IsCompleted, "Canceled enumeration was marked completed.");
            ClassicAssert.IsNull(result.NextToken, "Canceled enumeration produced a boundary token.");
            ClassicAssert.IsFalse(stream.IsDisposed, "Cancellation disposed the caller-owned stream.");
        }

        [Test]
        public async Task EnumerateEventsAsync_PreCanceledEnumerationDoesNotRead()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile()));
            await using var reader = await MidiFile.ReadLazyAsync(stream);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var result = reader.EnumerateEventsAsync();
            var readsBeforeEnumeration = stream.AsyncReadsCount;
            await using var enumerator = result.Events.GetAsyncEnumerator(cancellation.Token);

            Assert.CatchAsync<OperationCanceledException>(
                async () => await enumerator.MoveNextAsync(),
                "A pre-canceled token did not cancel enumeration.");

            ClassicAssert.AreEqual(readsBeforeEnumeration, stream.AsyncReadsCount, "Pre-canceled enumeration performed I/O.");
            ClassicAssert.IsFalse(result.IsCompleted, "Pre-canceled enumeration was marked completed.");
        }

        [Test]
        public async Task EnumerateEventsAsync_DisposedReaderFailureDoesNotComplete()
        {
            using var stream = new AsyncOnlyStream(GetBytes(new MidiFile()));
            var reader = await MidiFile.ReadLazyAsync(stream);
            var result = reader.EnumerateEventsAsync();
            await reader.DisposeAsync();
            await using var enumerator = result.Events.GetAsyncEnumerator();

            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await enumerator.MoveNextAsync(),
                "A disposed reader did not fail enumeration.");
            ClassicAssert.IsFalse(result.IsCompleted, "Failed enumeration was marked completed.");
        }

        [Test]
        public void EnumerateEventsAsync_NullReader()
        {
            ClassicAssert.Throws<ArgumentNullException>(
                () => MidiTokensReaderUtilities.EnumerateEventsAsync(null),
                "A null reader was accepted.");
        }

        [Test]
        public void EnumerateEvents_CompletionState([Values] bool stopEarly)
        {
            using var stream = new MemoryStream(GetBytes(new MidiFile(
                new TrackChunk(new TextEvent("first"), new TextEvent("second")))));
            using var reader = MidiFile.ReadLazy(stream);

            while (!(reader.ReadToken() is ChunkHeaderToken chunkHeader && chunkHeader.ChunkId == TrackChunk.Id))
            {
            }

            var result = reader.EnumerateEvents();
            ClassicAssert.IsFalse(result.IsCompleted, "An unstarted synchronous sequence was marked completed.");

            using (var enumerator = result.Events.GetEnumerator())
            {
                ClassicAssert.IsTrue(enumerator.MoveNext(), "The first synchronous event was not returned.");
                ClassicAssert.IsFalse(result.IsCompleted, "Yielding a synchronous event completed enumeration.");

                if (!stopEarly)
                {
                    while (enumerator.MoveNext())
                    {
                    }
                }
            }

            ClassicAssert.AreEqual(!stopEarly, result.IsCompleted, "Synchronous completion state is incorrect.");
        }

        [Test]
        public async Task EnumerateObjectsAsync_ReadDetectWriteTracks()
        {
            var originalFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 120 }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)64, (SevenBitNumber)0) { DeltaTime = 240 }));
            using var input = new AsyncOnlyStream(GetBytes(originalFile), false, 1);
            using var output = new AsyncOnlyStream();
            await using var reader = await MidiFile.ReadLazyAsync(input);
            var writer = await MidiFile.WriteLazyAsync(output);

            await using (writer)
            {
                MidiToken currentToken = await reader.ReadTokenAsync();

                while (currentToken != null)
                {
                    if (currentToken is ChunkHeaderToken chunkHeader && chunkHeader.ChunkId == TrackChunk.Id)
                    {
                        await writer.StartTrackChunkAsync();
                        var result = reader.EnumerateEventsAsync();

                        await using (var objectsWriter = new TimedObjectsWriter(writer))
                        {
                            await objectsWriter.WriteObjectsAsync(result.Events.EnumerateObjectsAsync(
                                ObjectType.Note | ObjectType.TimedEvent));
                        }

                        ClassicAssert.IsTrue(result.IsCompleted, "Object detection did not consume the track boundary.");
                        await writer.EndTrackChunkAsync();
                        currentToken = result.NextToken;
                    }
                    else
                        currentToken = await reader.ReadTokenAsync();
                }
            }

            using var restoredInput = new MemoryStream(output.ToArray());
            var restoredFile = MidiFile.Read(restoredInput);

            MidiAsserts.AreEqual(originalFile, restoredFile, false, "The asynchronous track pipeline changed MIDI data.");
        }

        private static byte[] GetBytes(MidiFile file)
        {
            using var stream = new MemoryStream();
            file.Write(stream);

            return stream.ToArray();
        }

        private static async Task MoveToTrackAsync(MidiTokensReader reader)
        {
            while (await reader.ReadTokenAsync() is MidiToken token)
            {
                if (token is ChunkHeaderToken chunkHeader && chunkHeader.ChunkId == TrackChunk.Id)
                    return;
            }

            ClassicAssert.Fail("The input contained no track header.");
        }

        private static async Task<List<MidiEvent>> CollectAsync(IAsyncEnumerable<MidiEvent> events)
        {
            var result = new List<MidiEvent>();

            await foreach (var midiEvent in events)
            {
                result.Add(midiEvent);
            }

            return result;
        }
    }
}

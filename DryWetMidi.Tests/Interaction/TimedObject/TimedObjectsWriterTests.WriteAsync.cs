using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class TimedObjectsWriterTests
    {
        [Test]
        public async Task WriteObjectsAsync_PreservesDeferredNoteOffs()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);

            await writer.WriteObjectsAsync(new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 100, 0),
                new Note((SevenBitNumber)64, 50, 10)
            });
            await writer.DisposeAsync();
            await tokensWriter.DisposeAsync();

            ClassicAssert.IsTrue(stream.CanRead, "Async object writing disposed the caller-owned stream.");

            stream.Position = 0;
            var notes = MidiFile.Read(stream).GetNotes();

            ClassicAssert.AreEqual(2, notes.Count, "Async object writing returned an invalid note count.");
            ClassicAssert.IsTrue(notes.Any(n => n.Time == 0 && n.Length == 100), "The first deferred note-off was not preserved.");
            ClassicAssert.IsTrue(notes.Any(n => n.Time == 10 && n.Length == 50), "The second deferred note-off was not preserved.");
        }

        [Test]
        public async Task EndTrackChunkAsync_FlushesObjectWriter()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);

            await writer.WriteObjectAsync(new Note((SevenBitNumber)60, 100, 0));
            await tokensWriter.EndTrackChunkAsync();
            await writer.DisposeAsync();
            await tokensWriter.DisposeAsync();

            stream.Position = 0;

            ClassicAssert.AreEqual(1, MidiFile.Read(stream).GetNotes().Count, "Ending the track did not flush the object writer.");
        }

        [Test]
        public async Task WriteObjectsAsync_AsyncProducerIsDisposed()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);
            var producerDisposed = false;

            async IAsyncEnumerable<ITimedObject> ProduceObjects()
            {
                try
                {
                    await Task.Yield();
                    yield return new Note((SevenBitNumber)60, 20, 0);
                    throw new InvalidOperationException("Producer failed.");
                }
                finally
                {
                    producerDisposed = true;
                }
            }

            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.WriteObjectsAsync(ProduceObjects()), "Async object writing did not propagate producer failure.");
            ClassicAssert.IsTrue(producerDisposed, "Failed async producer was not disposed.");

            await writer.DisposeAsync();
            await tokensWriter.DisposeAsync();
        }

        [Test]
        public async Task WriteObjectAsync_Canceled_DoesNotAdvance()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60, 20, 100), cancellation.Token), "Pre-canceled object writing did not throw.");
            ClassicAssert.AreEqual(0, tokensWriter.CurrentTime, "Canceled object writing advanced the token writer.");

            await writer.DisposeAsync();
            await tokensWriter.DisposeAsync();
        }

        [Test]
        public async Task DisposeAsync_RejectsSubsequentWrites()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);

            await writer.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => writer.WriteObject(new Note((SevenBitNumber)60)), "Disposed writer accepted an object.");
            Assert.Throws<ObjectDisposedException>(() => writer.WriteObjects(Array.Empty<ITimedObject>()), "Disposed writer accepted an object collection.");
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)), "Disposed writer accepted an async write.");

            await tokensWriter.DisposeAsync();
        }

        [Test]
        public async Task WriteObjectsAsync_RejectsOverlappingOperations()
        {
            using var stream = new MemoryStream();
            var tokensWriter = MidiFile.WriteLazy(stream);
            await tokensWriter.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokensWriter);
            var producerStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseProducer = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async IAsyncEnumerable<ITimedObject> ProduceObjects()
            {
                producerStarted.SetResult(true);
                await releaseProducer.Task;
                yield return new Note((SevenBitNumber)60, 20, 0);
            }

            var pendingWrite = writer.WriteObjectsAsync(ProduceObjects());
            await producerStarted.Task;

            Assert.Throws<InvalidOperationException>(() => writer.WriteObject(new Note((SevenBitNumber)60)), "Object writer accepted an overlapping synchronous write.");
            Assert.Throws<InvalidOperationException>(() => tokensWriter.EndTrackChunk(), "Track ending overlapped object writing.");
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)), "Object writer accepted an overlapping async write.");

            releaseProducer.SetResult(true);
            await pendingWrite;
            await writer.DisposeAsync();
            await tokensWriter.DisposeAsync();
        }
    }
}

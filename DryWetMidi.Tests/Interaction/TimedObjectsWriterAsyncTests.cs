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
    [TestFixture]
    public sealed class TimedObjectsWriterAsyncTests
    {
        [Test]
        public async Task WriteObjectsAsync_PreservesDeferredNoteOffs()
        {
            using var stream = new MemoryStream();
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            await writer.WriteObjectsAsync(new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 100, 0),
                new Note((SevenBitNumber)64, 50, 10)
            });
            await writer.DisposeAsync();
            await tokens.DisposeAsync();

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
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            await writer.WriteObjectAsync(new Note((SevenBitNumber)60, 100, 0));
            await tokens.EndTrackChunkAsync();
            await writer.DisposeAsync();
            await tokens.DisposeAsync();

            stream.Position = 0;

            ClassicAssert.AreEqual(1, MidiFile.Read(stream).GetNotes().Count, "Ending the track did not flush the object writer.");
        }

        [Test]
        public async Task WriteObjectsAsync_AsyncProducerIsDisposed()
        {
            using var stream = new MemoryStream();
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            var disposed = false;

            async IAsyncEnumerable<ITimedObject> Produce()
            {
                try
                {
                    await Task.Yield();
                    yield return new Note((SevenBitNumber)60, 20, 0);
                    throw new InvalidOperationException("Producer failed.");
                }
                finally
                {
                    disposed = true;
                }
            }

            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.WriteObjectsAsync(Produce()), "Async object writing did not propagate producer failure.");
            ClassicAssert.IsTrue(disposed, "Failed async producer was not disposed.");

            await writer.DisposeAsync();
            await tokens.DisposeAsync();
        }

        [Test]
        public async Task WriteObjectAsync_Canceled_DoesNotAdvance()
        {
            using var stream = new MemoryStream();
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            using var source = new CancellationTokenSource();
            source.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60, 20, 100), source.Token), "Pre-canceled object writing did not throw.");
            ClassicAssert.AreEqual(0, tokens.CurrentTime, "Canceled object writing advanced the token writer.");

            await writer.DisposeAsync();
            await tokens.DisposeAsync();
        }

        [Test]
        public async Task DisposeAsync_RejectsSubsequentWrites()
        {
            using var stream = new MemoryStream();
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            await writer.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => writer.WriteObject(new Note((SevenBitNumber)60)), "Disposed writer accepted an object.");
            Assert.Throws<ObjectDisposedException>(() => writer.WriteObjects(Array.Empty<ITimedObject>()), "Disposed writer accepted an object collection.");
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)), "Disposed writer accepted an async write.");

            await tokens.DisposeAsync();
        }

        [Test]
        public async Task WriteObjectsAsync_RejectsOverlappingOperations()
        {
            using var stream = new MemoryStream();
            var tokens = MidiFile.WriteLazy(stream);
            await tokens.StartTrackChunkAsync();
            var writer = new TimedObjectsWriter(tokens);
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async IAsyncEnumerable<ITimedObject> Produce()
            {
                started.SetResult(true);
                await release.Task;
                yield return new Note((SevenBitNumber)60, 20, 0);
            }

            var pending = writer.WriteObjectsAsync(Produce());
            await started.Task;

            Assert.Throws<InvalidOperationException>(() => writer.WriteObject(new Note((SevenBitNumber)60)), "Object writer accepted an overlapping synchronous write.");
            Assert.Throws<InvalidOperationException>(() => tokens.EndTrackChunk(), "Track ending overlapped object writing.");
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)), "Object writer accepted an overlapping async write.");

            release.SetResult(true);
            await pending;
            await writer.DisposeAsync();
            await tokens.DisposeAsync();
        }
    }
}

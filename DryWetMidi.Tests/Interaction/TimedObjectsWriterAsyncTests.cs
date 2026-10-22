using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

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

            Assert.That(stream.CanRead, Is.True);
            stream.Position = 0;
            var notes = MidiFile.Read(stream).GetNotes();
            Assert.That(notes.Count, Is.EqualTo(2));
            Assert.That(notes, Has.Some.Matches<Note>(n => n.Time == 0 && n.Length == 100));
            Assert.That(notes, Has.Some.Matches<Note>(n => n.Time == 10 && n.Length == 50));
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
            Assert.That(MidiFile.Read(stream).GetNotes().Count, Is.EqualTo(1));
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

            Assert.ThrowsAsync<InvalidOperationException>(async () => await writer.WriteObjectsAsync(Produce()));
            Assert.That(disposed, Is.True);
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
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60, 20, 100), source.Token));
            Assert.That(tokens.CurrentTime, Is.EqualTo(0));
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

            Assert.Throws<ObjectDisposedException>(() => writer.WriteObject(new Note((SevenBitNumber)60)));
            Assert.Throws<ObjectDisposedException>(() => writer.WriteObjects(Array.Empty<ITimedObject>()));
            Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)));
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
            Assert.Throws<InvalidOperationException>(() => writer.WriteObject(new Note((SevenBitNumber)60)));
            Assert.Throws<InvalidOperationException>(() => tokens.EndTrackChunk());
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await writer.WriteObjectAsync(new Note((SevenBitNumber)60)));
            release.SetResult(true);
            await pending;
            await writer.DisposeAsync();
            await tokens.DisposeAsync();
        }
    }
}

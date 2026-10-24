using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public async Task ProcessNotes_AwaitsSequentiallyAndPersistsTiming()
        {
            var track = CreateTrack();
            var active = 0;
            var visited = new List<long>();

            var count = await track.ProcessNotesAsync(async (note, token) =>
            {
                ClassicAssert.AreEqual(1, Interlocked.Increment(ref active), "Async note callbacks overlapped.");
                await Task.Delay(1, token);
                visited.Add(note.Time);
                note.Time += 25;
                note.Length += 5;
                Interlocked.Decrement(ref active);
            }, match: note => note.NoteNumber == (SevenBitNumber)60);

            ClassicAssert.AreEqual(2, count, "Invalid processed note count.");
            ClassicAssert.AreEqual(new[] { 0L, 200L }, visited, "Notes were visited out of order.");
            ClassicAssert.AreEqual(new[] { 0L, 25L, 225L }, track.GetNotes().Select(n => n.Time), "Processed note times are invalid.");
            ClassicAssert.AreEqual(new[] { 105L, 55L }, track.GetNotes().Where(n => n.NoteNumber == (SevenBitNumber)60).Select(n => n.Length), "Processed note lengths are invalid.");
        }

        [Test]
        public async Task ProcessNotes_HintNoneMatchesSynchronousBehavior()
        {
            var expected = CreateTrack();
            var actual = CreateTrack();

            expected.ProcessNotes(n => n.Time += 30, hint: NoteProcessingHint.None);
            await actual.ProcessNotesAsync(async (n, token) =>
            {
                await Task.Yield();
                n.Time += 30;
            }, hint: NoteProcessingHint.None);

            MidiAsserts.AreEqual(expected.GetTimedEvents(), actual.GetTimedEvents(), "Hint");
        }

        [Test]
        public void AsyncActionCancellation_StopsBeforeNextCallback()
        {
            using var cancellation = new CancellationTokenSource();
            var callbacks = 0;

            Assert.ThrowsAsync<OperationCanceledException>(async () => await CreateTrack().ProcessNotesAsync(async (note, token) =>
            {
                await Task.Yield();
                callbacks++;
                cancellation.Cancel();
            }, cancellationToken: cancellation.Token), "Cancellation from a note callback did not stop processing.");
            ClassicAssert.AreEqual(1, callbacks, "A callback ran after cancellation.");
        }

        [Test]
        public async Task Progress_ReportsCompletionAndWork()
        {
            var progress = new ImmediateProgress();

            await CreateTrack().GetNotesAsync(progress: progress);

            ClassicAssert.IsTrue(progress.Reports.Last().IsCompleted, "Note detection did not report completion.");
            ClassicAssert.Greater(progress.Reports.Last().ProcessedItems, 0, "Note detection reported no work.");
        }

        [Test]
        public async Task GetNotesAsync_ReturnsWhileWorkerIsBlockedAtDeterministicGate()
        {
            using var release = new ManualResetEventSlim();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IEnumerable<MidiEvent> Produce()
            {
                entered.SetResult(true);
                release.Wait();
                yield return new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100);
                yield return new NoteOffEvent((SevenBitNumber)60, SevenBitNumber.MinValue) { DeltaTime = 10 };
            }

            var operation = Produce().GetNotesAsync();
            try
            {
                await entered.Task;
                ClassicAssert.IsFalse(operation.IsCompleted, "The caller must regain control while source enumeration is blocked on the worker.");
                TestContext.Out.WriteLine("Responsiveness: async invocation returned while its worker was held at a deterministic source gate; no elapsed-time threshold was used.");
            }
            finally
            {
                release.Set();
            }

            ClassicAssert.AreEqual(1, (await operation).Count, "The released worker detected an invalid note count.");
        }

        [Test]
        [NonParallelizable]
        public async Task GetNotesAsync_ReportsSeparateThroughputAllocationAndRetainedMemoryDiagnostics()
        {
            const int notesCount = 8192;
            var events = new MidiEvent[notesCount * 2];

            for (var i = 0; i < notesCount; i++)
            {
                events[2 * i] = new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100) { DeltaTime = 1 };
                events[2 * i + 1] = new NoteOffEvent((SevenBitNumber)60, SevenBitNumber.MinValue) { DeltaTime = 10 };
            }

            events.Take(2).GetNotes();
            await events.Take(2).GetNotesAsync();

            var syncRetainedBefore = GC.GetTotalMemory(true);
            var syncAllocatedBefore = GC.GetTotalAllocatedBytes(true);
            var syncTimer = Stopwatch.StartNew();
            var syncNotes = events.GetNotes();
            syncTimer.Stop();
            var syncAllocated = GC.GetTotalAllocatedBytes(true) - syncAllocatedBefore;
            var syncRetained = GC.GetTotalMemory(true) - syncRetainedBefore;
            GC.KeepAlive(syncNotes);

            var asyncRetainedBefore = GC.GetTotalMemory(true);
            var asyncAllocatedBefore = GC.GetTotalAllocatedBytes(true);
            var asyncTimer = Stopwatch.StartNew();
            var asyncNotes = await events.GetNotesAsync();
            asyncTimer.Stop();
            var asyncAllocated = GC.GetTotalAllocatedBytes(true) - asyncAllocatedBefore;
            var asyncRetained = GC.GetTotalMemory(true) - asyncRetainedBefore;
            GC.KeepAlive(asyncNotes);

            TestContext.Out.WriteLine($"GetNotes input: {events.Length} events, {notesCount} notes.");
            TestContext.Out.WriteLine($"Sync throughput observation: {syncTimer.Elapsed.TotalMilliseconds:F3} ms, {notesCount / Math.Max(syncTimer.Elapsed.TotalSeconds, 1e-9):F0} notes/s.");
            TestContext.Out.WriteLine($"Async throughput observation: {asyncTimer.Elapsed.TotalMilliseconds:F3} ms, {notesCount / Math.Max(asyncTimer.Elapsed.TotalSeconds, 1e-9):F0} notes/s.");
            TestContext.Out.WriteLine($"Process-wide allocation deltas: sync={syncAllocated} bytes; async={asyncAllocated} bytes.");
            TestContext.Out.WriteLine($"Post-GC retained-memory deltas with each result rooted: sync={syncRetained} bytes; async={asyncRetained} bytes.");
            TestContext.Out.WriteLine("Diagnostics are single-run observations, include process/test-host noise, and make no speedup, peak-memory, or bounded-memory claim. Throughput excludes retained-memory snapshot collection time.");

            ClassicAssert.AreEqual(notesCount, asyncNotes.Count, "Diagnostic async note count is invalid.");
            MidiAsserts.AreEqual(syncNotes, asyncNotes, "Diagnostic operations must retain equivalent results");
        }
    }
}

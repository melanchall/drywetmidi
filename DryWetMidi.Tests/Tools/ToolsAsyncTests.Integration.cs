using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class ToolsAsyncTests
    {
        [Test]
        public async Task CpuFamilies_ResultsAndHooksMatchSync()
        {
            var file = CreateFile();

            ClassicAssert.AreEqual(file.TakePart((MidiTimeSpan)50).GetNotes().Count, (await file.TakePartAsync((MidiTimeSpan)50)).GetNotes().Count, "Async take-part note count differs.");
            ClassicAssert.AreEqual(file.TakePart((MidiTimeSpan)10, (MidiTimeSpan)50).GetNotes().Count, (await file.TakePartAsync((MidiTimeSpan)10, (MidiTimeSpan)50)).GetNotes().Count, "Async ranged take-part note count differs.");
            ClassicAssert.AreEqual(file.SkipPart((MidiTimeSpan)50).GetNotes().Count, (await file.SkipPartAsync((MidiTimeSpan)50)).GetNotes().Count, "Async skip-part note count differs.");
            ClassicAssert.AreEqual(file.CutPart((MidiTimeSpan)10, (MidiTimeSpan)50).GetNotes().Count, (await file.CutPartAsync((MidiTimeSpan)10, (MidiTimeSpan)50)).GetNotes().Count, "Async cut-part note count differs.");

            var files = new[] { file, file };

            ClassicAssert.AreEqual(files.MergeSequentially().GetNotes().Count, (await files.MergeSequentiallyAsync()).GetNotes().Count, "Async sequential merge note count differs.");
            ClassicAssert.AreEqual(files.MergeSimultaneously().GetNotes().Count, (await files.MergeSimultaneouslyAsync()).GetNotes().Count, "Async simultaneous merge note count differs.");
            ClassicAssert.AreEqual(file.GetNotes().MergeObjects(TempoMap.Default).Count(), (await Collect(file.GetNotes().MergeObjectsAsync(TempoMap.Default))).Count, "Async object merge count differs.");

            var repeater = new CustomRepeater();
            var repeated = await repeater.RepeatAsync(file.GetNotes(), 2, TempoMap.Default);

            ClassicAssert.AreEqual(file.GetNotes().Repeat(2, TempoMap.Default).Count, repeated.Count, "Async repeating note count differs.");
            ClassicAssert.Greater(repeater.Calls, 0, "Async repeating did not invoke the custom hook.");

            var progress = new InlineProgress();

            await file.ResizeAsync(2.0, progress);

            ClassicAssert.Greater(progress.Reports.Count, 0, "Async resizing did not report progress.");

            await file.GetNotes().ResizeObjectsGroupAsync(0.5, TempoMap.Default);
            await file.QuantizeObjectsAsync(ObjectType.Note, new SteppedGrid((MidiTimeSpan)10));
            await file.SanitizeAsync();
        }

        [Test]
        public void Cpu_PreCanceledOperations_DoNotMutate()
        {
            var file = CreateFile();
            var originalLength = file.GetNotes().First().Length;
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(() => file.ResizeAsync(2.0, cancellationToken: cancellation.Token), "Pre-canceled resizing did not throw.");
            Assert.CatchAsync<OperationCanceledException>(() => file.RepeatAsync(2, cancellationToken: cancellation.Token), "Pre-canceled repeating did not throw.");
            Assert.CatchAsync<OperationCanceledException>(() => file.SanitizeAsync(cancellationToken: cancellation.Token), "Pre-canceled sanitizing did not throw.");
            ClassicAssert.AreEqual(originalLength, file.GetNotes().First().Length, "Pre-canceled operations changed the note length.");
        }
    }
}

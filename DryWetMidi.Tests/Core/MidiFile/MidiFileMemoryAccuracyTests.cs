using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    // GC.GetTotalMemory is process-wide, so this fixture is non-parallelizable: NUnit runs it alone,
    // without other tests affecting the measurements.
    [TestFixture]
    [NonParallelizable]
    public sealed class MidiFileMemoryAccuracyTests
    {

        private const double MaxMemorySizeAccuracyWindow = 0.05;

        private static IEnumerable<TestCaseData> GetAccuracyCases()
        {
            const int count = 50000;

            foreach (var midiEvent in MidiFileTests.GetEventsForMaxMemorySize())
            {
                var e = midiEvent;
                yield return new TestCaseData(
                    (Func<MidiChunk[]>)(() => new MidiChunk[] { new TrackChunk(Enumerable.Range(0, count).Select(_ => e.Clone())) }),
                    count).SetName($"Event_{e.GetType().Name}");
            }

            foreach (var length in new[] { 10, 1000 })
            {
                var l = length;
                yield return new TestCaseData(
                    (Func<MidiChunk[]>)(() => new MidiChunk[] { new TrackChunk(Enumerable.Range(0, 5000).Select(_ => new TextEvent(new string('a', l)))) }),
                    5000).SetName($"TextEvent_{l}");
                yield return new TestCaseData(
                    (Func<MidiChunk[]>)(() => new MidiChunk[] { new TrackChunk(Enumerable.Range(0, 5000).Select(_ => new NormalSysExEvent(new byte[l]))) }),
                    5000).SetName($"SysEx_{l}");
                yield return new TestCaseData(
                    (Func<MidiChunk[]>)(() => Enumerable.Range(0, 5000).Select(_ => (MidiChunk)new UnknownChunk("Abcd") { Data = new byte[l] }).ToArray()),
                    5000).SetName($"UnknownChunk_{l}");
            }

            yield return new TestCaseData(
                (Func<MidiChunk[]>)(() => Enumerable.Range(0, 10000).Select(_ => (MidiChunk)new TrackChunk()).ToArray()),
                10000).SetName("TrackChunk_Empty");
        }

        [TestCaseSource(nameof(GetAccuracyCases))]
        public void Read_MaxMemorySize_AccuracyOfEstimation(Func<MidiChunk[]> createChunks, int itemsCount)
        {
            Assume.That(IntPtr.Size, Is.EqualTo(8), "Accuracy window is defined for 64-bit.");

            var bytes = MidiFileTests.GetFileBytes(createChunks(), null);
            var estimate = MidiFileTests.GetMinimalMemoryLimit(bytes);

            var actuals = new List<long>();
            for (var i = 0; i < 5; i++)
            {
                actuals.Add(MeasureReading(bytes));
            }

            var actual = actuals.OrderBy(a => a).ElementAt(actuals.Count / 2);
            var ratio = (double)estimate / actual;

            TestContext.Out.WriteLine($"Estimated: {(double)estimate / itemsCount:F1} B/item, actual: {(double)actual / itemsCount:F1} B/item, ratio: {ratio:F3}");

            ClassicAssert.That(ratio, Is.InRange(1 - MaxMemorySizeAccuracyWindow, 1 + MaxMemorySizeAccuracyWindow));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static long MeasureReading(byte[] bytes)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var before = GC.GetTotalMemory(true);
            var holder = new object[1];
            holder[0] = MidiFile.Read(new MemoryStream(bytes));
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(holder);

            return after - before;
        }
    }
}

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

        private static MidiFile? _heldFile;

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

            TestContext.Out.WriteLine($"Repeats (B/item): {string.Join(", ", actuals.Select(a => ((double)a / itemsCount).ToString("F1")))}");
            TestContext.Out.WriteLine($"Estimated: {(double)estimate / itemsCount:F1} B/item, actual: {(double)actual / itemsCount:F1} B/item, ratio: {ratio:F3}");

            ClassicAssert.That(ratio, Is.InRange(1 - MaxMemorySizeAccuracyWindow, 1 + MaxMemorySizeAccuracyWindow));
        }

        [Test]
        public void MeasurementCalibration()
        {
            const int count = 50000;

            var instanceSize = MeasureInstanceSize();
            var expected = (long)count * (instanceSize + IntPtr.Size);

            var actuals = new List<long>();
            for (var i = 0; i < 5; i++)
            {
                var weak = BuildAndHold(count);
                CollectGarbage();
                var withGraph = GC.GetTotalMemory(true);
                _heldGraph = null;
                CollectGarbage();
                var withoutGraph = GC.GetTotalMemory(true);
                ClassicAssert.IsFalse(weak.IsAlive);
                actuals.Add(withGraph - withoutGraph);
            }

            var actual = actuals.OrderBy(a => a).ElementAt(actuals.Count / 2);
            TestContext.Out.WriteLine($"Calibration: instance {instanceSize} B, expected {(double)expected / count:F1} B/item, actual {(double)actual / count:F1} B/item");

            ClassicAssert.That((double)expected / actual, Is.InRange(1 - MaxMemorySizeAccuracyWindow, 1 + MaxMemorySizeAccuracyWindow));
        }

        private sealed class CalibrationObject
        {
            public long A;
            public long B;
        }

        private static List<CalibrationObject>? _heldGraph;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static long MeasureInstanceSize()
        {
            new CalibrationObject();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var o = new CalibrationObject();
            var size = GC.GetAllocatedBytesForCurrentThread() - before;
            GC.KeepAlive(o);
            return size;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference BuildAndHold(int count)
        {
            var list = new List<CalibrationObject>(count);
            for (var i = 0; i < count; i++)
            {
                list.Add(new CalibrationObject());
            }

            _heldGraph = list;
            return new WeakReference(list);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static long MeasureReading(byte[] bytes)
        {
            var weakFile = ReadAndHold(bytes);

            // Temporary objects of the reading are collected, so only the file graph is left
            CollectGarbage();
            var withFile = GC.GetTotalMemory(true);

            _heldFile = null;
            CollectGarbage();
            var withoutFile = GC.GetTotalMemory(true);

            ClassicAssert.IsFalse(weakFile.IsAlive, "The file must be collected to measure its size.");

            return withFile - withoutFile;
        }

        // No reference to the file is left in the frame of the caller, so it can't be kept alive
        // by an untracked local or a register (as in unoptimized code or on some platforms)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ReadAndHold(byte[] bytes)
        {
            _heldFile = MidiFile.Read(new MemoryStream(bytes));
            return new WeakReference(_heldFile);
        }

        private static void CollectGarbage()
        {
            for (var i = 0; i < 3; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
            }
        }
    }
}

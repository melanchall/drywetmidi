using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    [NonParallelizable]
    [Explicit("Diagnostic repro for GC.GetTotalMemory behavior; prints numbers, never fails")]
    public sealed class GcMeasurementReproTests
    {
        private sealed class Obj
        {
            public long A;
            public long B;
        }

        private static object? _held;

        [Test]
        public void Repro()
        {
            var o = TestContext.Out;
            o.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
            o.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            o.WriteLine($"Process arch: {RuntimeInformation.ProcessArchitecture}, OS arch: {RuntimeInformation.OSArchitecture}");
            o.WriteLine($"ServerGC: {GCSettings.IsServerGC}, LatencyMode: {GCSettings.LatencyMode}");
            foreach (var name in new[] { "DOTNET_gcServer", "DOTNET_gcConcurrent", "DOTNET_GCgen0size", "DOTNET_GCConserveMemory", "DOTNET_GCHeapAffinitizeMask", "DOTNET_TieredCompilation" })
            {
                o.WriteLine($"{name}={Environment.GetEnvironmentVariable(name) ?? "<not set>"}");
            }

            o.WriteLine($"sizeof(IntPtr)={IntPtr.Size}");
            o.WriteLine($"Allocated size of one Obj (GC.GetAllocatedBytesForCurrentThread): {AllocatedSize(() => new Obj())} B");
            o.WriteLine($"Allocated size of new byte[0]: {AllocatedSize(() => new byte[0])} B");
            o.WriteLine($"Allocated size of new byte[100]: {AllocatedSize(() => new byte[100])} B");
            o.WriteLine();

            o.WriteLine("Retained size measured by GC.GetTotalMemory (graph held in a static field, then released):");
            o.WriteLine("case | expected B | measured B (5 runs)");

            Measure("1 Obj", 32, () => new Obj());
            Measure("Obj[1] with one Obj", 24 + 8 + 32, () => new[] { new Obj() });
            Measure("byte[1000]", 24 + 1000, () => new byte[1000]);
            Measure("byte[100000]", 24 + 100000, () => new byte[100000]);
            Measure("Obj[1000] filled", 24 + 1000 * (8 + 32), () => Fill(1000));
            Measure("Obj[50000] filled", 24 + 50000 * (8 + 32), () => Fill(50000));
            Measure("Obj[1000000] filled", 24 + 1000000L * (8 + 32), () => Fill(1000000));
        }

        private static Obj[] Fill(int count)
        {
            var array = new Obj[count];
            for (var i = 0; i < count; i++)
            {
                array[i] = new Obj();
            }

            return array;
        }

        private static void Measure(string name, long expected, Func<object> create)
        {
            var results = new long[5];
            for (var i = 0; i < results.Length; i++)
            {
                results[i] = MeasureOnce(create);
            }

            TestContext.Out.WriteLine($"{name} | {expected} | {string.Join(", ", results)}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static long MeasureOnce(Func<object> create)
        {
            Hold(create);

            CollectGarbage();
            var withObject = GC.GetTotalMemory(true);

            _held = null;
            CollectGarbage();
            var withoutObject = GC.GetTotalMemory(true);

            return withObject - withoutObject;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Hold(Func<object> create) => _held = create();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static long AllocatedSize(Func<object> create)
        {
            create();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var result = create();
            var size = GC.GetAllocatedBytesForCurrentThread() - before;
            GC.KeepAlive(result);
            return size;
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

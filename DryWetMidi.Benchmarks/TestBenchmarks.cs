using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks
{
    [TestFixture]
    public sealed class TestBenchmarks : BenchmarkTest
    {
        [ClrJob]
        public class Benchmarks_Test
        {
            [Benchmark]
            public void Run()
            {
                Thread.Sleep(1000);
            }
        }

        [Test]
        public void Run()
        {
            RunBenchmarks<Benchmarks_Test>();
        }
    }
}

using System.Threading;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks
{
    [TestFixture]
    public sealed class TestBenchmarks : BenchmarkTest
    {
        [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net472)]
        public class Benchmarks_Test
        {
            [Benchmark]
            public void Run()
            {
                WaitOperations.Wait(1000);
            }
        }

        [Test]
        public void Run()
        {
            RunBenchmarks<Benchmarks_Test>();
        }
    }
}

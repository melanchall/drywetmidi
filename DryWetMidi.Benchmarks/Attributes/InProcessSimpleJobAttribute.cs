using System;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Melanchall.DryWetMidi.Benchmarks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class InProcessSimpleJobAttribute : JobConfigBaseAttribute
    {
        private const int DefaultValue = -1;

        public InProcessSimpleJobAttribute(
            RunStrategy runStrategy,
            int launchCount = DefaultValue,
            int warmupCount = DefaultValue,
            int targetCount = DefaultValue,
            int invocationCount = DefaultValue,
            string id = null)
            : base(CreateJob(id, launchCount, warmupCount, targetCount, invocationCount, runStrategy))
        {
        }

        private static Job CreateJob(string id, int launchCount, int warmupCount, int targetCount, int invocationCount, RunStrategy? runStrategy)
        {
            var job = Job.InProcess.UnfreezeCopy();

            if (id != null)
                job = job.WithId(id);

            if (launchCount != DefaultValue)
                job = job.WithLaunchCount(launchCount);

            if (warmupCount != DefaultValue)
                job = job.WithWarmupCount(warmupCount);

            if (targetCount != DefaultValue)
                job = job.WithTargetCount(targetCount);

            if (invocationCount != DefaultValue)
                job = job.WithInvocationCount(invocationCount);

            if (runStrategy != null)
                job.Run.RunStrategy = runStrategy.Value;

            return job;
        }
    }
}

using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Melanchall.CheckDwmApi
{
    internal abstract class RunTickGeneratorTask<TTickGenerator> : ITask
        where TTickGenerator : TickGenerator, new()
    {
        private readonly int _ticksCount;

        protected RunTickGeneratorTask(
            int ticksCount)
        {
            _ticksCount = ticksCount;
        }

        public abstract string GetTitle();

        public abstract string GetDescription();

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            RunTicksGenerator(1, reportWriter);
            RunTicksGenerator(10, reportWriter);
        }

        private void RunTicksGenerator(
            int intervalMs,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle($"Running '{typeof(TTickGenerator).Name}' with interval {intervalMs} ms...");

            using var tickGenerator = new TTickGenerator();

            reportWriter.WriteOperationSubTitle("preparing");

            var stopwatch = new Stopwatch();
            var times = new List<long>();

            tickGenerator.TickGenerated += (_, _) =>
            {
                times.Add(stopwatch.ElapsedMilliseconds);
            };

            reportWriter.WriteOperationSubTitle($"running collecting times ({_ticksCount} ticks will be fired)");

            tickGenerator.TryStart(TimeSpan.FromMilliseconds(intervalMs));
            stopwatch.Start();

            var timeout = TimeSpan.FromSeconds(10);
            var success = SpinWait.SpinUntil(() => times.Count >= _ticksCount, timeout);
            if (!success)
                throw new TaskFailedException("Tick generation timed out.");

            tickGenerator.TryStop();

            reportWriter.WriteOperationSubTitle("calculating deltas");

            var deltas = new List<long>();
            for (int i = 1; i < times.Count; i++)
            {
                deltas.Add(times[i] - times[i - 1]);
            }

            reportWriter.WriteOperationSubTitle("calculating statistics");

            var min = deltas.Min();
            reportWriter.WriteOperationSubTitle($"min = {min} ms");

            var max = deltas.Max();
            reportWriter.WriteOperationSubTitle($"max = {max} ms");

            var median = deltas.OrderBy(d => d).ElementAt(deltas.Count / 2);
            reportWriter.WriteOperationSubTitle($"median = {median} ms");

            var average = deltas.Average();
            reportWriter.WriteOperationSubTitle($"average = {average:F2} ms");

            int[] percentsAreas = [5, 10, 20, 50];

            foreach (var percent in percentsAreas)
            {
                var lowerBound = (int)Math.Round(intervalMs * (1 - percent / 100.0));
                var upperBound = (int)Math.Round(intervalMs * (1 + percent / 100.0));
                var percentInArea = deltas.Count(d => d >= lowerBound && d <= upperBound) * 100.0 / deltas.Count;
                reportWriter.WriteOperationSubTitle($"{percent} % area ({lowerBound} ms - {upperBound} ms) = {percentInArea:F2} %");
            }
        }
    }
}

using System;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks
{
    public abstract class BenchmarkTest
    {
        #region Properties

        public TestContext TestContext { get; set; }

        #endregion

        #region Methods

        [SetUp]
        public void SetupTest()
        {
#if DEBUG
            Assert.Inconclusive("Unable to run benchmarks on Debug configuration. Use Release.");
#endif

            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        protected void RunBenchmarks<TBenchmarks>()
        {
            RunBenchmarks(typeof(TBenchmarks));
        }

        protected void RunBenchmarks(Type type, params IColumn[] columns)
        {
            var summary = BenchmarkRunner.Run(
                type,
                ManualConfig.Create(DefaultConfig.Instance)
                            .With(AsciiDocExporter.Default, JsonExporter.Brief)
                            .With(StatisticColumn.Min)
                            .With(columns));

            // Assert validation errors

            var validationErrorsStringBuilder = new StringBuilder();

            foreach (var error in summary.ValidationErrors)
            {
                validationErrorsStringBuilder.AppendLine($"Validation error (critical={error.IsCritical}): {error.Message}");
            }

            var validationError = validationErrorsStringBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(validationError))
                Assert.Inconclusive(validationError);

            // Assert build/generate/execute errors

            var buildErrorsStringBuilder = new StringBuilder();

            foreach (var report in summary.Reports)
            {
                var buildResult = report.BuildResult;

                if (!buildResult.IsBuildSuccess)
                    buildErrorsStringBuilder.AppendLine($"Build exception={buildResult.BuildException.Message}");

                if (!buildResult.IsGenerateSuccess)
                    buildErrorsStringBuilder.AppendLine($"Generate exception={buildResult.GenerateException.Message}");

                foreach (var executeResult in report.ExecuteResults)
                {
                    if (executeResult.ExitCode == 0)
                        continue;

                    buildErrorsStringBuilder.AppendLine($"Execute result: exit code is not 0");
                }
            }

            var buildError = buildErrorsStringBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(buildError))
                Assert.Inconclusive(buildError);
        }

        #endregion
    }
}

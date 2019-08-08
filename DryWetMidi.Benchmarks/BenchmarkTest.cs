using System;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
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
                            .With(AsciiDocExporter.Default)
                            .With(columns));

            // Assert validation errors

            var validationErrorsStringBuilder = new StringBuilder();

            foreach (var error in summary.ValidationErrors)
            {
                var benchmarkDisplayInfo = error.Benchmark?.DisplayInfo;
                var isCritical = error.IsCritical;
                var message = error.Message;

                validationErrorsStringBuilder.AppendLine($"[{benchmarkDisplayInfo} | Critical={isCritical}]: {message}. ");
            }

            var validationError = validationErrorsStringBuilder.ToString().Trim();

            Assert.IsEmpty(validationError, validationError);
        }

        #endregion
    }
}

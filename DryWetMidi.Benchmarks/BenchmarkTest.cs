using System;
using System.Text;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
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
            var summary = BenchmarkRunner.Run<TBenchmarks>(ManualConfig.Create(DefaultConfig.Instance)
                                                                       .With(JsonExporter.Brief));

            // Assert validation errors

            var validationErrorsStringBuilder = new StringBuilder();

            foreach (var error in summary.ValidationErrors)
            {
                var benchmarkDisplayInfo = error.BenchmarkCase?.DisplayInfo;
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

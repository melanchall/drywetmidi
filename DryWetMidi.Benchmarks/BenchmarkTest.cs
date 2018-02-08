using System;
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
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        #endregion
    }
}

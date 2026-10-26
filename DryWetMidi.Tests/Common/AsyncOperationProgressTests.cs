using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class AsyncOperationProgressTests
    {
        [Test]
        public void ToString_IncludesCountsAndCompletion(
            [Values(null, 10L)] long? totalItems,
            [Values] bool isCompleted)
        {
            var progress = new AsyncOperationProgress(5, isCompleted, "Reading", totalItems);
            var expectedCounts = totalItems.HasValue ? "5/10" : "5/?";
            var expectedCompletion = isCompleted ? "True" : "False";

            var actual = progress.ToString();

            ClassicAssert.AreEqual($"Reading: {expectedCounts}; completed: {expectedCompletion}", actual, "Progress text omitted or changed the phase, counts, or completion state.");
        }
    }
}

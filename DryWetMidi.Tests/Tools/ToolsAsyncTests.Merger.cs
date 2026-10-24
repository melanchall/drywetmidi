using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class ToolsAsyncTests
    {
        [Test]
        public void Merger_Cancellation_StopsPreparationBeforeFirstResult()
        {
            using var cancellation = new CancellationTokenSource();
            int visited = 0;
            bool disposed = false;
            IEnumerable<ITimedObject> Source()
            {
                try
                {
                    while (true)
                    {
                        if (++visited == 4)
                            cancellation.Cancel();

                        yield return new Note((SevenBitNumber)60, 100, visited);
                    }
                }
                finally
                {
                    disposed = true;
                }
            }

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await Collect(Source().MergeObjectsAsync(TempoMap.Default, cancellationToken: cancellation.Token)), "Merging did not observe cancellation during preparation.");
            ClassicAssert.AreEqual(4, visited, "Merging continued source enumeration after cancellation.");
            ClassicAssert.IsTrue(disposed, "Canceled merging did not dispose the source.");
        }
    }
}

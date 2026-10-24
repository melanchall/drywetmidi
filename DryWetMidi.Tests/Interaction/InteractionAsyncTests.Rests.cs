using System;
using System.Collections.Generic;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public void CancellationDuringRestsSorting_StopsSourceEnumeration()
        {
            using var cancellation = new CancellationTokenSource();
            var visited = 0;
            IEnumerable<ITimedObject> Produce()
            {
                while (visited < 100000)
                {
                    if (++visited == 100)
                        cancellation.Cancel();

                    yield return new Note((SevenBitNumber)60, 1, 100000 - visited);
                }
            }

            Assert.ThrowsAsync<OperationCanceledException>(async () => await Produce().GetRestsAsync(cancellationToken: cancellation.Token), "Rest detection did not observe cancellation.");
            ClassicAssert.AreEqual(100, visited, "Rest source enumeration continued after cancellation.");
        }
    }
}

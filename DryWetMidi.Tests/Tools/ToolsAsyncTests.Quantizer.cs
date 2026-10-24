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
        [TestCase(1, false)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(2, true)]
        public void Quantize_CancelsBeforeFiltering(int canceledPass, bool nullItems)
        {
            using var cancellation = new CancellationTokenSource();
            var pass = 0;
            var visited = new int[2];
            IEnumerable<ITimedObject> Objects()
            {
                pass++;
                for (var i = 0; i < 1000; i++)
                {
                    visited[pass - 1]++;
                    if (pass == canceledPass && nullItems)
                        cancellation.Cancel();

                    yield return pass == canceledPass && nullItems ? null : new Note((SevenBitNumber)60);
                }
            }

            var settings = new QuantizingSettings
            {
                Filter = obj =>
                {
                    if (pass != canceledPass)
                        return true;

                    cancellation.Cancel();
                    return false;
                }
            };

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await new Quantizer().QuantizeAsync(
                    Objects(),
                    new SteppedGrid(new MidiTimeSpan(10)),
                    TempoMap.Default,
                    settings,
                    cancellationToken: cancellation.Token), "Quantizing did not observe cancellation before filtering.");
            ClassicAssert.LessOrEqual(visited[canceledPass - 1], 2, "Quantizing continued enumeration after cancellation.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class AsyncOperationTests
    {
        private sealed class Progress : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();

            public void Report(AsyncOperationProgress value)
            {
                Reports.Add(value);
            }
        }

        [Test]
        public async Task Run_ReportsCompletion()
        {
            var progress = new Progress();
            var result = await AsyncOperation.Run(
                () =>
                {
                    AsyncOperation.Checkpoint();
                    AsyncOperation.Checkpoint();

                    return 42;
                },
                CancellationToken.None,
                progress);

            Assert.That(result, Is.EqualTo(42));
            Assert.That(progress.Reports.Count, Is.EqualTo(1));
            Assert.That(progress.Reports[0].ProcessedItems, Is.EqualTo(2));
            Assert.That(progress.Reports[0].IsCompleted, Is.True);
        }

        [Test]
        public void Run_PreCanceled_DoesNotInvokeAction()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var invoked = false;

            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await AsyncOperation.Run(
                    () =>
                    {
                        invoked = true;
                    },
                    source.Token));

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Run_CanceledInsideLoop_DoesNotReportCompletion()
        {
            using var source = new CancellationTokenSource();
            var progress = new Progress();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await AsyncOperation.Run(
                    () =>
                    {
                        source.Cancel();
                        AsyncOperation.Checkpoint();
                    },
                    source.Token,
                    progress));

            Assert.That(progress.Reports, Is.Empty);
        }

        [Test]
        public async Task Run_DoesNotLeakCancellationContext()
        {
            using var source = new CancellationTokenSource();
            await AsyncOperation.Run(() => AsyncOperation.Checkpoint(), source.Token);
            source.Cancel();

            Assert.DoesNotThrow(() => AsyncOperation.Checkpoint());
            await AsyncOperation.Run(() => AsyncOperation.Checkpoint(), CancellationToken.None);
        }

        [Test]
        public async Task Run_AsyncDelegate_ReportsAfterAwaitedWork()
        {
            var progress = new Progress();
            var result = await AsyncOperation.Run(
                async () =>
                {
                    await Task.Yield();
                    AsyncOperation.Checkpoint();
                    Assert.That(progress.Reports, Is.Empty);

                    return 42;
                },
                CancellationToken.None,
                progress);

            Assert.That(result, Is.EqualTo(42));
            Assert.That(progress.Reports.Count, Is.EqualTo(1));
            Assert.That(progress.Reports[0].ProcessedItems, Is.EqualTo(1));
            Assert.That(progress.Reports[0].IsCompleted, Is.True);
        }
    }
}

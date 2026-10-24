using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
        public void Progress_ToString_IncludesCountsAndCompletion(
            [Values(null, 10L)] long? totalItems,
            [Values] bool isCompleted)
        {
            var progress = new AsyncOperationProgress(5, isCompleted, "Reading", totalItems);
            var expectedCounts = totalItems.HasValue ? "5/10" : "5/?";
            var expectedCompletion = isCompleted ? "True" : "False";

            var actual = progress.ToString();

            ClassicAssert.AreEqual($"Reading: {expectedCounts}; completed: {expectedCompletion}", actual, "Progress text omitted or changed the phase, counts, or completion state.");
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

            ClassicAssert.AreEqual(42, result, "Async operation returned an invalid result.");
            ClassicAssert.AreEqual(1, progress.Reports.Count, "Async operation did not report exactly one completion.");
            ClassicAssert.AreEqual(2, progress.Reports[0].ProcessedItems, "Async operation reported an invalid work count.");
            ClassicAssert.IsTrue(progress.Reports[0].IsCompleted, "Async operation did not report successful completion.");
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
                    source.Token), "Pre-canceled operation did not throw.");

            ClassicAssert.IsFalse(invoked, "Pre-canceled operation invoked the action.");
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
                    progress), "Cancellation at a checkpoint did not throw.");

            ClassicAssert.IsEmpty(progress.Reports, "Canceled operation reported completion.");
        }

        [Test]
        public async Task Run_DoesNotLeakCancellationContext()
        {
            using var source = new CancellationTokenSource();
            await AsyncOperation.Run(() => AsyncOperation.Checkpoint(), source.Token);
            source.Cancel();

            ClassicAssert.DoesNotThrow(() => AsyncOperation.Checkpoint(), "Cancellation context leaked outside the operation.");
            await AsyncOperation.Run(() => AsyncOperation.Checkpoint(), CancellationToken.None);
        }

        [Test]
        public async Task Run_OnlyCreatesContextWhenNeeded(
            [Values] bool cancelable,
            [Values] bool reportProgress,
            [Values] bool awaitedDelegate)
        {
            using var cancellation = new CancellationTokenSource();
            var token = cancelable ? cancellation.Token : CancellationToken.None;
            var progress = reportProgress ? new Progress() : null;
            var expectedActive = cancelable || reportProgress;

            void CheckContext()
            {
                ClassicAssert.AreEqual(expectedActive, AsyncOperation.IsActive, "Operation context activity does not match the cancellation/progress requirements.");

                var source = new[] { 1, 2 };
                var checkpointItems = InteractionAsyncUtilities.CheckpointItems(source);

                ClassicAssert.AreEqual(!expectedActive, ReferenceEquals(source, checkpointItems), "Checkpoint enumeration did not select the expected direct-source or active-context path.");
            }

            ClassicAssert.IsFalse(AsyncOperation.IsActive, "An operation context leaked into the caller before invocation.");

            int result;
            if (awaitedDelegate)
            {
                result = await AsyncOperation.Run(
                    async () =>
                    {
                        CheckContext();
                        await Task.Yield();
                        CheckContext();

                        return 42;
                    },
                    token,
                    progress);
            }
            else
            {
                result = await AsyncOperation.Run(
                    () =>
                    {
                        CheckContext();

                        return 42;
                    },
                    token,
                    progress);
            }

            ClassicAssert.AreEqual(42, result, "Context selection changed the operation result.");
            ClassicAssert.IsFalse(AsyncOperation.IsActive, "The operation context leaked into the caller after completion.");
        }

        [Test]
        public async Task Run_NestedContextlessOperationRestoresParent([Values] bool awaitedDelegate)
        {
            using var cancellation = new CancellationTokenSource();
            var progress = new Progress();

            var result = await AsyncOperation.Run(
                async () =>
                {
                    ClassicAssert.IsTrue(AsyncOperation.IsActive, "The parent operation has no cancellation/progress context.");
                    AsyncOperation.Checkpoint();

                    if (awaitedDelegate)
                    {
                        await AsyncOperation.Run(
                            async () =>
                            {
                                ClassicAssert.IsFalse(AsyncOperation.IsActive, "The contextless child inherited its parent's context.");
                                await Task.Yield();
                                ClassicAssert.IsFalse(AsyncOperation.IsActive, "The contextless child acquired a context after awaiting.");
                                AsyncOperation.Checkpoint(100);

                                return 0;
                            },
                            CancellationToken.None);
                    }
                    else
                    {
                        await AsyncOperation.Run(
                            () =>
                            {
                                ClassicAssert.IsFalse(AsyncOperation.IsActive, "The contextless child inherited its parent's context.");
                                AsyncOperation.Checkpoint(100);
                            },
                            CancellationToken.None);
                    }

                    ClassicAssert.IsTrue(AsyncOperation.IsActive, "The contextless child did not restore its parent's active context.");
                    AsyncOperation.Checkpoint();

                    return 42;
                },
                cancellation.Token,
                progress);

            ClassicAssert.AreEqual(42, result, "Nested context selection changed the parent operation result.");
            ClassicAssert.AreEqual(2, progress.Reports[progress.Reports.Count - 1].ProcessedItems, "The child changed parent progress or the restored parent context lost progress.");
            ClassicAssert.IsTrue(progress.Reports[progress.Reports.Count - 1].IsCompleted, "The restored parent context did not report completion.");
            ClassicAssert.IsFalse(AsyncOperation.IsActive, "The nested operation left an active context in the caller.");
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
                    ClassicAssert.IsEmpty(progress.Reports, "Progress reported completion before awaited work finished.");

                    return 42;
                },
                CancellationToken.None,
                progress);

            ClassicAssert.AreEqual(42, result, "Awaited operation returned an invalid result.");
            ClassicAssert.AreEqual(1, progress.Reports.Count, "Awaited operation did not report exactly one completion.");
            ClassicAssert.AreEqual(1, progress.Reports[0].ProcessedItems, "Awaited operation reported an invalid work count.");
            ClassicAssert.IsTrue(progress.Reports[0].IsCompleted, "Awaited operation did not report successful completion.");
        }
    }
}

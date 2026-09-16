using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    [NonParallelizable]
    public sealed class MidiOperationsExecutorTests
    {
        [Test]
        public void ExecuteOperation_InitializesApartmentOnWorkerThread()
        {
            var initializationThreadId = 0;
            var operationThreadId = 0;

            using var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () =>
                {
                    initializationThreadId = Environment.CurrentManagedThreadId;
                    return 0;
                });

            executor.ExecuteOperation(() =>
            {
                operationThreadId = Environment.CurrentManagedThreadId;
            });

            ClassicAssert.AreNotEqual(
                Environment.CurrentManagedThreadId,
                operationThreadId,
                "Operation was executed on the calling thread.");

            ClassicAssert.AreEqual(
                initializationThreadId,
                operationThreadId,
                "Apartment initialization and the native operation ran on different threads.");
        }

        [Test]
        public void ExecuteOperation_UsesSingleWorkerForConcurrentCalls()
        {
            var workerThreadIds = new HashSet<int>();
            var lockObject = new object();

            using var executor = MidiOperationsExecutor.CreateForTests(useWorkerThread: true);

            Parallel.For(
                0,
                100,
                _ =>
                {
                    executor.ExecuteOperation(() =>
                    {
                        lock (lockObject)
                            workerThreadIds.Add(Environment.CurrentManagedThreadId);
                    });
                });

            ClassicAssert.AreEqual(
                1,
                workerThreadIds.Count,
                "Operations were executed by more than one worker thread.");
        }

        [Test]
        public void ExecuteOperation_ReentrantCallExecutesInlineOnWorker()
        {
            var outerThreadId = 0;
            var innerThreadId = 0;

            using var executor = MidiOperationsExecutor.CreateForTests(useWorkerThread: true);

            executor.ExecuteOperation(() =>
            {
                outerThreadId = Environment.CurrentManagedThreadId;

                executor.ExecuteOperation(() =>
                {
                    innerThreadId = Environment.CurrentManagedThreadId;
                });
            });

            ClassicAssert.AreNotEqual(
                0,
                outerThreadId,
                "Outer operation was not executed.");

            ClassicAssert.AreEqual(
                outerThreadId,
                innerThreadId,
                "A nested operation was queued instead of executing inline on the worker.");
        }

        [Test]
        public void ExecuteOperation_PropagatesOperationException()
        {
            using var executor = MidiOperationsExecutor.CreateForTests(useWorkerThread: true);

            var exception = ClassicAssert.Throws<InvalidOperationException>(() =>
            {
                executor.ExecuteOperation(() =>
                    throw new InvalidOperationException("Expected test exception."));
            });

            ClassicAssert.AreEqual("Expected test exception.", exception!.Message);
        }

        [Test]
        public void ExecuteOperation_PropagatesApartmentInitializationException()
        {
            using var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () =>
                    throw new InvalidOperationException("Apartment initialization failed."));

            var exception = ClassicAssert.Throws<InvalidOperationException>(() =>
            {
                executor.ExecuteOperation(() => { });
            });

            ClassicAssert.AreEqual("Apartment initialization failed.", exception!.Message);
        }

        [Test]
        public void UseDirectExecution_DrainsExistingJobsBeforeReturning()
        {
            using var firstOperationStarted = new ManualResetEventSlim(false);
            using var allowFirstOperationToFinish = new ManualResetEventSlim(false);
            using var secondOperationExecuted = new ManualResetEventSlim(false);

            using var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () => 0);

            var firstOperation = Task.Run(() =>
            {
                executor.ExecuteOperation(() =>
                {
                    firstOperationStarted.Set();
                    allowFirstOperationToFinish.Wait();
                });
            });

            firstOperationStarted.Wait();

            var secondOperation = Task.Run(() =>
            {
                executor.ExecuteOperation(() => secondOperationExecuted.Set());
            });

            ClassicAssert.IsTrue(
                SpinWait.SpinUntil(
                    () => executor.QueuedJobsCount == 1,
                    TimeSpan.FromSeconds(5)),
                "The second operation was not queued.");

            var switchToDirectExecution = Task.Run(executor.UseDirectExecution);

            ClassicAssert.IsFalse(
                switchToDirectExecution.Wait(TimeSpan.FromMilliseconds(100)),
                "Worker was retired before its queued operation completed.");

            allowFirstOperationToFinish.Set();

            ClassicAssert.IsTrue(
                switchToDirectExecution.Wait(TimeSpan.FromSeconds(5)),
                "Timed out while retiring the worker.");

            ClassicAssert.IsTrue(
                firstOperation.Wait(TimeSpan.FromSeconds(5)),
                "First queued operation did not complete.");

            ClassicAssert.IsTrue(
                secondOperation.Wait(TimeSpan.FromSeconds(5)),
                "Second queued operation did not complete.");

            ClassicAssert.IsTrue(
                secondOperationExecuted.IsSet,
                "Queued operation was dropped while switching to direct execution.");

            ClassicAssert.IsFalse(
                executor.IsWorkerThreadUsed,
                "Worker is still reported as active after switching to direct execution.");
        }

        [Test]
        public void UseDirectExecution_SubsequentOperationRunsOnCallingThread()
        {
            using var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () => 0);

            executor.ExecuteOperation(() => { });
            executor.UseDirectExecution();

            var callingThreadId = Environment.CurrentManagedThreadId;
            var operationThreadId = 0;

            executor.ExecuteOperation(() =>
                operationThreadId = Environment.CurrentManagedThreadId);

            ClassicAssert.AreEqual(
                callingThreadId,
                operationThreadId,
                "Operation still ran on a worker after WinMM direct execution was selected.");
        }

        [Test]
        public void Dispose_FailsQueuedOperationInsteadOfLeavingCallerBlocked()
        {
            using var firstOperationStarted = new ManualResetEventSlim(false);
            using var allowFirstOperationToFinish = new ManualResetEventSlim(false);

            var executor = MidiOperationsExecutor.CreateForTests(useWorkerThread: true);

            try
            {
                var firstOperation = Task.Run(() =>
                {
                    executor.ExecuteOperation(() =>
                    {
                        firstOperationStarted.Set();
                        allowFirstOperationToFinish.Wait();
                    });
                });

                ClassicAssert.IsTrue(
                    firstOperationStarted.Wait(TimeSpan.FromSeconds(5)),
                    "The first operation did not start.");

                var queuedOperation = Task.Run(() =>
                {
                    return ClassicAssert.Throws<ObjectDisposedException>(() =>
                        executor.ExecuteOperation(() => { }));
                });

                ClassicAssert.IsTrue(
                    SpinWait.SpinUntil(
                        () => executor.QueuedJobsCount == 1,
                        TimeSpan.FromSeconds(5)),
                    "The second operation was not queued.");

                var disposeTask = Task.Run(executor.Dispose);

                ClassicAssert.IsTrue(
                    executor.DisposeStartedWaitHandle.WaitOne(TimeSpan.FromSeconds(5)),
                    "Dispose did not begin.");

                // The executor has now marked itself disposed and drained _jobs, but it
                // remains blocked in Join until the currently running first job exits.
                ClassicAssert.IsFalse(
                    disposeTask.Wait(TimeSpan.FromMilliseconds(100)),
                    "Dispose returned before the running worker operation was released.");

                allowFirstOperationToFinish.Set();

                ClassicAssert.IsTrue(
                    disposeTask.Wait(TimeSpan.FromSeconds(5)),
                    "Timed out while disposing the executor.");

                ClassicAssert.IsTrue(
                    firstOperation.Wait(TimeSpan.FromSeconds(5)),
                    "Running operation did not complete.");

                ClassicAssert.IsTrue(
                    queuedOperation.Wait(TimeSpan.FromSeconds(5)),
                    "Queued operation remained blocked during disposal.");

                ClassicAssert.IsNotNull(
                    queuedOperation.Result,
                    "Queued operation did not receive ObjectDisposedException.");
            }
            finally
            {
                allowFirstOperationToFinish.Set();
                executor.Dispose();
            }
        }

        [Test]
        public void WorkerShutdown_UninitializesApartmentOnWorkerThread()
        {
            var initializationThreadId = 0;
            var uninitializationThreadId = 0;

            using (var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () =>
                {
                    initializationThreadId = Environment.CurrentManagedThreadId;
                    return 0;
                },
                uninitializeApartment: () => uninitializationThreadId = Environment.CurrentManagedThreadId))
            {
                executor.ExecuteOperation(() => { });
            }

            ClassicAssert.AreNotEqual(
                0,
                initializationThreadId,
                "Apartment was not initialized.");

            ClassicAssert.AreEqual(
                initializationThreadId,
                uninitializationThreadId,
                "Apartment was uninitialized on a different thread.");
        }

        [Test]
        public void ExecuteOperation_ThrowsWhenApartmentInitializationReturnsError()
        {
            const int initializationErrorCode = unchecked((int)0x80010106);

            using var executor = MidiOperationsExecutor.CreateForTests(
                useWorkerThread: true,
                initializeApartment: () => initializationErrorCode);

            var exception = ClassicAssert.Throws<NativeApiException>(() =>
            {
                executor.ExecuteOperation(() => { });
            });

            ClassicAssert.AreEqual(
                initializationErrorCode,
                exception!.MainErrorCode,
                "Invalid apartment initialization error code.");
        }
    }
}
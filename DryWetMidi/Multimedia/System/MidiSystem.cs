using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiSystem : IDisposable
    {
        private static readonly Lazy<MidiSystem> _instance =
        new(() => new MidiSystem(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static MidiSystem Instance => _instance.Value;

        // 1. We wrap the Action and its completion signal into a structural container
        private readonly ConcurrentQueue<MidiJob> _jobQueue = new();
        private readonly AutoResetEvent _wakeUpEvent = new(false);
        private readonly Thread? _workerThread;
        private readonly bool _needThread;
        private volatile bool _isRunning = true;

        private const long SpinDurationTicks = TimeSpan.TicksPerMillisecond * 1;
        private static readonly double StopwatchTicksPerCacheTick = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        private MidiSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _needThread = true;

            if (_needThread)
            {
                _workerThread = new Thread(ProcessMidiQueue)
                {
                    Name = "DryWetMidi.GlobalNativeWorker",
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _workerThread.Start();
            }
        }

        /// <summary>
        /// Enqueues a native P/Invoke call and BLOCKS the calling thread until the work is executed.
        /// </summary>
        public void EnqueueNativeCall(Action nativeAction)
        {
            if (!_isRunning) throw new ObjectDisposedException(nameof(MidiSystem));

            if (!_needThread)
            {
                // If we don't need a dedicated thread, we can just execute the action directly
                nativeAction();
                return;
            }

            // 2. Create a lightweight, stack-allocated signal
            using (var signal = new ManualResetEventSlim(false))
            {
                var job = new MidiJob(nativeAction, signal);

                _jobQueue.Enqueue(job);
                _wakeUpEvent.Set(); // Wake up worker if sleeping

                // 3. Block the calling thread right here until the worker thread executes the job
                signal.Wait();
            }
        }

        public TResult EnqueueNativeCall<TResult>(Func<TResult> nativeAction)
        {
            if (!_isRunning) throw new ObjectDisposedException(nameof(MidiSystem));

            if (!_needThread)
            {
                // If we don't need a dedicated thread, we can just execute the action directly
                return nativeAction();
            }

            using (var signal = new ManualResetEventSlim(false))
            {
                // Define a variable to capture the result securely across the thread boundary
                TResult result = default!;
                Exception nativeException = null!;

                var job = new MidiJob(() =>
                {
                    try
                    {
                        result = nativeAction();
                    }
                    catch (Exception ex)
                    {
                        // Capture any exception so it can be re-thrown on the calling thread
                        nativeException = ex;
                    }
                }, signal);

                _jobQueue.Enqueue(job);
                _wakeUpEvent.Set();

                // Block the calling thread right here
                signal.Wait();

                // If the native call crashed, throw the exception on the thread that requested it
                if (nativeException != null)
                {
                    throw new System.Reflection.TargetInvocationException("An error occurred during native execution.", nativeException);
                }

                return result;
            }
        }

        private void ProcessMidiQueue()
        {
            while (_isRunning)
            {
                while (_jobQueue.TryDequeue(out var job))
                {
                    try
                    {
                        // 4. Execute the P/Invoke payload
                        job.Action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MIDI Thread Exception: {ex.Message}");
                    }
                    finally
                    {
                        // 5. CRITICAL: Unblock the calling thread immediately
                        job.CompletionSignal.Set();
                    }
                }

                if (!_isRunning) break;

                if (ShouldSpinWaitForNextJob())
                {
                    continue;
                }

                if (_isRunning && _jobQueue.IsEmpty)
                {
                    _wakeUpEvent.WaitOne();
                }
            }
        }

        private bool ShouldSpinWaitForNextJob()
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            int spinCount = 0;

            while ((Stopwatch.GetTimestamp() - startTimestamp) < (SpinDurationTicks * StopwatchTicksPerCacheTick))
            {
                if (!_jobQueue.IsEmpty) return true;

                spinCount++;
                if (spinCount < 10)
                    Thread.SpinWait(1 << spinCount);
                else
                    Thread.Yield();
            }

            return false;
        }

        public void Dispose()
        {
            if (_workerThread == null)
                return;

            if (!_isRunning) return;
            _isRunning = false;
            _wakeUpEvent.Set();

            if (_workerThread?.IsAlive == true)
            {
                _workerThread.Join(TimeSpan.FromMilliseconds(500));
            }

            _wakeUpEvent.Dispose();
        }

        // A lightweight internal structural representation of our payload
        private readonly struct MidiJob
        {
            public readonly Action Action;
            public readonly ManualResetEventSlim CompletionSignal;

            public MidiJob(Action action, ManualResetEventSlim completionSignal)
            {
                Action = action;
                CompletionSignal = completionSignal;
            }
        }
    }
}

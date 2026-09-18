using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiOperationsExecutor : IDisposable
    {
        private readonly ConcurrentQueue<MidiJob> _jobs = new();
        private readonly AutoResetEvent _wakeUpEvent = new(false);
        private readonly Thread? _workerThread;
        private bool _needThread;
        private volatile bool _isRunning = true;

        private const long SpinDurationTicks = TimeSpan.TicksPerMillisecond * 10;
        private static readonly double StopwatchTicksPerCacheTick = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        private static readonly Lazy<MidiOperationsExecutor> _instance =
            new(() => new MidiOperationsExecutor());

        private MidiOperationsExecutor()
        {
            _needThread =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                LibraryConfiguration.UseWindowsMidiServices &&
                LibraryConfiguration.UseWorkerThreadForWindowsMidiServices;

            if (!_needThread)
                return;

            var processName = Process.GetCurrentProcess().ProcessName;
            _workerThread = new Thread(ProcessCallsQueue)
            {
                Name = $"DryWetMIDI (MIDI API worker): {processName}",
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
        }

        public static MidiOperationsExecutor Instance => _instance.Value;

        public void UseDirectExecution()
        {
            if (_workerThread == null)
                return;
            
            _wakeUpEvent.Set();
            if (_workerThread?.IsAlive == true)
                _workerThread.Join(TimeSpan.FromMilliseconds(500));

            _wakeUpEvent.Dispose();

            _needThread = false;
        }

        public void ExecuteOperation(Action nativeAction)
        {
            if (!_isRunning)
                throw new ObjectDisposedException(nameof(MidiOperationsExecutor));

            if (!_needThread)
            {
                nativeAction();
                return;
            }

            ExecuteOperation(() =>
            {
                nativeAction();
                return true;
            });
        }

        public TResult ExecuteOperation<TResult>(Func<TResult> nativeAction)
            where TResult : struct
        {
            if (!_isRunning)
                throw new ObjectDisposedException(nameof(MidiOperationsExecutor));

            if (!_needThread)
                return nativeAction();

            if (_workerThread?.IsAlive != true)
                _workerThread?.Start();

            using (var signal = new ManualResetEventSlim(false))
            {
                TResult result = default;
                Exception? nativeException = null;

                // TODO: check exception handling
                var job = new MidiJob(() =>
                {
                    try
                    {
                        result = nativeAction();
                    }
                    catch (Exception ex)
                    {
                        nativeException = ex;
                    }
                }, signal);

                _jobs.Enqueue(job);
                _wakeUpEvent.Set();

                signal.Wait();

                // TODO: wrap to another exception type?
                if (nativeException != null)
                    throw nativeException;

                return result;
            }
        }

        private void ProcessCallsQueue()
        {
            CommonApi.Api_InitializeWindowsApartment();

            while (_isRunning)
            {
                while (_jobs.TryDequeue(out var job))
                {
                    try
                    {
                        job.Action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        // TODO: log exception
                        Debug.WriteLine($"MIDI Thread Exception: {ex.Message}");
                    }
                    finally
                    {
                        job.CompletionSignal.Set();
                    }
                }

                if (!_isRunning)
                    break;

                if (ShouldSpinWaitForNextJob())
                    continue;

                if (_isRunning && _jobs.IsEmpty)
                    _wakeUpEvent.WaitOne();
            }
        }

        private bool ShouldSpinWaitForNextJob()
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var spinCount = 0;

            while ((Stopwatch.GetTimestamp() - startTimestamp) < (SpinDurationTicks * StopwatchTicksPerCacheTick))
            {
                if (!_jobs.IsEmpty)
                    return true;

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

            if (!_isRunning)
                return;
            
            _isRunning = false;
            _wakeUpEvent.Set();

            if (_workerThread?.IsAlive == true)
            {
                _workerThread.Join(TimeSpan.FromMilliseconds(500));
            }

            _wakeUpEvent.Dispose();
        }
    }
}

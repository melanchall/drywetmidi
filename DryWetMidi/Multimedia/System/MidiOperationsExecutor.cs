using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiOperationsExecutor : IDisposable
    {
#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _instanceLock = new();
#else
        private static readonly object _instanceLock = new();
#endif

        private static Lazy<MidiOperationsExecutor> _instance =
            new(() => new MidiOperationsExecutor());

#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _lockObject = new();
#else
        private static readonly object _lockObject = new();
#endif

        private readonly Queue<MidiJob> _jobs = new();
        private readonly AutoResetEvent _wakeUpEvent = new(false);
        private readonly ManualResetEventSlim _directExecutionReady = new(true);
        private readonly Thread? _workerThread;

        private bool _needThread;

        private bool _workerStarted;
        private bool _transitioningToDirectExecution;
        private bool _retireWorker;
        private bool _disposed;

        private volatile int _workerThreadId;
        private ExceptionDispatchInfo? _workerException;

        private readonly Func<int> _initializeApartment;
        private readonly Action _uninitializeApartment;

#if TEST
        private readonly ManualResetEventSlim _disposeStarted = new(false);

        public WaitHandle DisposeStartedWaitHandle => _disposeStarted.WaitHandle;
#endif

        private MidiOperationsExecutor()
            : this(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                LibraryConfiguration.UseWindowsMidiServices &&
                LibraryConfiguration.UseWorkerThread,
                CommonApi.Api_InitializeWindowsApartment,
                CommonApi.Api_UninitializeWindowsApartment)
        {
        }

        private MidiOperationsExecutor(
            bool needThread,
            Func<int> initializeApartment,
            Action uninitializeApartment)
        {
            _needThread = needThread;
            _initializeApartment = initializeApartment;
            _uninitializeApartment = uninitializeApartment;

            if (!_needThread)
                return;

            _workerThread = new Thread(ProcessCallsQueue)
            {
                Name = "DryWetMIDI MIDI API worker",
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
        }

        public static MidiOperationsExecutor Instance => _instance.Value;

        public int QueuedJobsCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _jobs.Count;
                }
            }
        }

        public bool IsWorkerThreadUsed
        {
            get
            {
                lock (_lockObject)
                {
                    return
                        _needThread &&
                        !_transitioningToDirectExecution &&
                        _workerStarted &&
                        _workerThread?.IsAlive == true &&
                        _workerException == null;
                }
            }
        }

        public static MidiOperationsExecutor CreateForTests(
            bool useWorkerThread,
            Func<int>? initializeApartment = null,
            Action? uninitializeApartment = null)
        {
            return new MidiOperationsExecutor(
                useWorkerThread,
                initializeApartment ?? (() => 0),
                uninitializeApartment ?? (() => { }));
        }

        public static void ResetInstance()
        {
            Lazy<MidiOperationsExecutor> oldInstance;

            lock (_instanceLock)
            {
                oldInstance = _instance;
                _instance = new Lazy<MidiOperationsExecutor>(() => new MidiOperationsExecutor());
            }

            if (oldInstance.IsValueCreated)
                oldInstance.Value.Dispose();
        }

        public void ExecuteOperation(Action nativeAction)
        {
            ExecuteOperation(() =>
            {
                nativeAction();
                return true;
            });
        }

        public TResult ExecuteOperation<TResult>(Func<TResult> nativeAction)
            where TResult : struct
        {
            if (Thread.CurrentThread.ManagedThreadId == _workerThreadId)
                return nativeAction();

            while (true)
            {
                MidiJob? job = null;
                ManualResetEventSlim? directExecutionReady = null;
                var executeDirectly = false;
                ExceptionDispatchInfo? workerException = null;

                lock (_lockObject)
                {
                    ThrowIfDisposed();

                    if (_workerException != null)
                    {
                        workerException = _workerException;
                    }
                    else if (!_needThread)
                    {
                        executeDirectly = true;
                    }
                    else if (_transitioningToDirectExecution)
                    {
                        directExecutionReady = _directExecutionReady;
                    }
                    else
                    {
                        StartWorkerIfNeeded();

                        if (_workerException != null)
                        {
                            workerException = _workerException;
                        }
                        else
                        {
                            job = new MidiJob(() => nativeAction());
                            _jobs.Enqueue(job);
                            _wakeUpEvent.Set();
                        }
                    }
                }

                if (workerException != null)
                    workerException.Throw();

                if (executeDirectly)
                    return nativeAction();

                if (directExecutionReady != null)
                {
                    directExecutionReady.Wait();
                    continue;
                }

                return job!.WaitAndGetResult<TResult>();
            }
        }

        public void UseDirectExecution()
        {
            if (Thread.CurrentThread.ManagedThreadId == _workerThreadId)
            {
                // TODO: use unexpected exception type
                throw new InvalidOperationException(
                    "Direct execution cannot be enabled from the MIDI worker thread.");
            }

            Thread? workerThread;

            lock (_lockObject)
            {
                ThrowIfDisposed();

                if (!_needThread)
                    return;

                if (_workerException != null)
                    _workerException.Throw();

                _transitioningToDirectExecution = true;
                _directExecutionReady.Reset();

                if (!_workerStarted)
                {
                    _needThread = false;
                    _transitioningToDirectExecution = false;
                    _directExecutionReady.Set();
                    return;
                }

                _retireWorker = true;
                _wakeUpEvent.Set();
                workerThread = _workerThread;
            }

            if (Thread.CurrentThread.ManagedThreadId == _workerThreadId)
                return;

            workerThread!.Join();

            lock (_lockObject)
            {
                _workerStarted = false;
                _needThread = false;
                _transitioningToDirectExecution = false;
                _directExecutionReady.Set();
            }
        }

        private void StartWorkerIfNeeded()
        {
            if (_workerStarted || !_needThread)
                return;

            try
            {
                _workerThread!.Start();
                _workerStarted = true;
            }
            catch (Exception exception)
            {
                _workerException = ExceptionDispatchInfo.Capture(exception);
                throw;
            }
        }

        private void ProcessCallsQueue()
        {
            _workerThreadId = Thread.CurrentThread.ManagedThreadId;

            var apartmentInitialized = false;

            try
            {
                var initializationResult = _initializeApartment();
                if (initializationResult != 0)
                {
                    throw new NativeApiException(
                        "Unable to initialize the WinRT apartment.",
                        initializationResult,
                        0);
                }

                apartmentInitialized = true;

                while (true)
                {
                    MidiJob? job = null;

                    lock (_lockObject)
                    {
                        if (_disposed)
                            return;

                        if (_jobs.Count > 0)
                        {
                            job = _jobs.Dequeue();
                        }
                        else if (_retireWorker)
                        {
                            return;
                        }
                    }

                    if (job != null)
                    {
                        job.Execute();
                        continue;
                    }

                    _wakeUpEvent.WaitOne();
                }
            }
            catch (Exception exception)
            {
                FaultWorker(exception);
            }
            finally
            {
                if (apartmentInitialized)
                    _uninitializeApartment();

                _workerThreadId = 0;
            }
        }

        private void FaultWorker(Exception exception)
        {
            List<MidiJob> pendingJobs;

            lock (_lockObject)
            {
                _workerException = ExceptionDispatchInfo.Capture(exception);
                pendingJobs = [.. _jobs];
                _jobs.Clear();

                _transitioningToDirectExecution = false;
                _directExecutionReady.Set();
            }

            foreach (var job in pendingJobs)
            {
                job.Fail(exception);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiOperationsExecutor));
        }

        public void Dispose()
        {
            Thread? workerThread;
            List<MidiJob> pendingJobs;
            var disposeWakeUpEvent = false;

            lock (_lockObject)
            {
                if (_disposed)
                    return;

                _disposed = true;

#if TEST
                _disposeStarted.Set();
#endif

                _retireWorker = true;
                _transitioningToDirectExecution = false;
                _directExecutionReady.Set();

                pendingJobs = new List<MidiJob>(_jobs);
                _jobs.Clear();

                workerThread = _workerStarted ? _workerThread : null;

                if (workerThread != null)
                    _wakeUpEvent.Set();
                else
                    disposeWakeUpEvent = true;
            }

            var disposedException = new ObjectDisposedException(nameof(MidiOperationsExecutor));

            foreach (var job in pendingJobs)
            {
                job.Fail(disposedException);
            }

            if (workerThread != null &&
                Thread.CurrentThread.ManagedThreadId != _workerThreadId)
            {
                workerThread.Join();
                disposeWakeUpEvent = true;
            }

            if (disposeWakeUpEvent)
                _wakeUpEvent.Dispose();

            _directExecutionReady.Dispose();
        }
    }
}
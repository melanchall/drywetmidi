using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiJob
    {
        private readonly Func<object> _operation;
        private readonly ManualResetEventSlim _completionSignal = new(false);

        private object? _result;
        private ExceptionDispatchInfo? _exception;

        public MidiJob(Func<object> operation)
        {
            _operation = operation;
        }

        public void Execute()
        {
            try
            {
                _result = _operation();
            }
            catch (Exception exception)
            {
                _exception = ExceptionDispatchInfo.Capture(exception);
            }
            finally
            {
                _completionSignal.Set();
            }
        }

        public void Fail(Exception exception)
        {
            _exception = ExceptionDispatchInfo.Capture(exception);
            _completionSignal.Set();
        }

        public TResult WaitAndGetResult<TResult>()
            where TResult : struct
        {
            _completionSignal.Wait();

            try
            {
                _exception?.Throw();
                return (TResult)_result!;
            }
            finally
            {
                _completionSignal.Dispose();
            }
        }
    }
}

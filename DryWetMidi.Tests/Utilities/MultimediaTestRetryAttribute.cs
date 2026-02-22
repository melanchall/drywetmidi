using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class MultimediaTestRetryAttribute : PropertyAttribute, IRepeatTest
    {
        private readonly int _count = 3;
        private readonly int _delayInMilliseconds = 10000;

        public TestCommand Wrap(TestCommand command)
        {
            return new RepeatedTestCommand(command, _count, _delayInMilliseconds);
        }

        public class RepeatedTestCommand : DelegatingTestCommand
        {
            private readonly TestCommand _innerCommand;
            private readonly int _repeatCount;
            private readonly int _delayInMilliseconds;

            public RepeatedTestCommand(
                TestCommand innerCommand,
                int repeatCount,
                int delayInMilliseconds)
                : base(innerCommand)
            {
                _innerCommand = innerCommand;
                _repeatCount = repeatCount;
                _delayInMilliseconds = delayInMilliseconds;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var count = _repeatCount;

                while (count-- > 0)
                {
                    try
                    {
                        context.CurrentResult = _innerCommand.Execute(context);
                    }
                    catch (Exception ex)
                    {
                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                        context.CurrentResult.RecordException(ex);
                    }

                    var resultState = context.CurrentResult.ResultState;

                    if (resultState != ResultState.Error &&
                        resultState != ResultState.Failure &&
                        resultState != ResultState.Cancelled)
                    {
                        break;
                    }

                    if (count > 0)
                    {
                        var attemptNumber = _repeatCount - count;

                        if (_delayInMilliseconds > 0)
                        {
                            TestContext.Progress.WriteLine(
                                $"[RetryWithDelay] Test failed on attempt {attemptNumber}/{_repeatCount}. " +
                                $"Waiting {_delayInMilliseconds}ms before retry...");

                            Thread.Sleep(_delayInMilliseconds);
                        }

                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                        context.CurrentRepeatCount++;
                    }
                }

                return context.CurrentResult;
            }
        }
    }
}

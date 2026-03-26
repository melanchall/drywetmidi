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
        private static readonly int RepeatsCount = 5;
        private static readonly TimeSpan Delay = TimeSpan.FromSeconds(10);

        public TestCommand Wrap(TestCommand command)
        {
            return new RepeatedTestCommand(command, RepeatsCount, Delay);
        }

        public class RepeatedTestCommand : DelegatingTestCommand
        {
            private readonly TestCommand _innerCommand;
            private readonly int _repeatCount;
            private readonly TimeSpan _delay;

            public RepeatedTestCommand(
                TestCommand innerCommand,
                int repeatCount,
                TimeSpan delay)
                : base(innerCommand)
            {
                _innerCommand = innerCommand;
                _repeatCount = repeatCount;
                _delay = delay;
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

                        if (_delay > TimeSpan.Zero)
                        {
                            TestContext.Progress.WriteLine(
                                $"Test failed on attempt {attemptNumber}/{_repeatCount}. Waiting {_delay} before retry...");

                            Thread.Sleep(_delay);
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

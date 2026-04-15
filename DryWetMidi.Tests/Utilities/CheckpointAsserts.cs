using Melanchall.DryWetMidi.Common;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class CheckpointAsserts
    {
        #region Constants

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        #endregion

        #region Methods

        public static void CheckCheckpointsAreNotReached(this TestCheckpoints checkpoints, params string[] checkpointsNames)
        {
            string[] GetReachedCheckpoints() =>
                checkpointsNames.Where(checkpoints.IsCheckpointReached).ToArray();

            var success = WaitOperations.Wait(() => !GetReachedCheckpoints().Any(), Timeout);
            ClassicAssert.IsTrue(
                success,
                $"Some checkpoints are unexpectedly reached: {string.Join(", ", GetReachedCheckpoints())}{Environment.NewLine}" +
                GetErrorsString(checkpoints));
        }

        public static void CheckCheckpointsReached(this TestCheckpoints checkpoints, params string[] checkpointsNames)
        {
            string[] GetReachedCheckpoints() =>
                checkpointsNames.Where(checkpoints.IsCheckpointReached).ToArray();

            var success = WaitOperations.Wait(() => checkpointsNames.SequenceEqual(GetReachedCheckpoints()), Timeout);
            ClassicAssert.IsTrue(
                success,
                $"Some checkpoints are not reached: {string.Join(", ", checkpointsNames.Except(GetReachedCheckpoints()))}" +
                GetErrorsString(checkpoints));
        }

        private static string GetErrorsString(TestCheckpoints checkpoints)
        {
            var errors = checkpoints.GetErrors().ToArray();
            return errors.Any()
                ? $"Errors reached:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(e => $"- {e}"))}"
                : "No errors reached.";
        }

        #endregion
    }
}

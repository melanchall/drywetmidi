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
                $"Some checkpoints are reached: {string.Join(", ", GetReachedCheckpoints())}");
        }

        public static void CheckCheckpointsReached(this TestCheckpoints checkpoints, params string[] checkpointsNames)
        {
            string[] GetReachedCheckpoints() =>
                checkpointsNames.Where(checkpoints.IsCheckpointReached).ToArray();

            var success = WaitOperations.Wait(() => checkpointsNames.SequenceEqual(checkpointsNames), Timeout);
            ClassicAssert.IsTrue(
                success,
                $"Some checkpoints are not reached: {string.Join(", ", checkpointsNames.Except(GetReachedCheckpoints()))}");
        }

        #endregion
    }
}

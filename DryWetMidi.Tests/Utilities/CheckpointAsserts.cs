using Melanchall.DryWetMidi.Common;
using NUnit.Framework.Legacy;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class CheckpointAsserts
    {
        #region Methods

        public static void CheckCheckpointsAreNotReached(this TestCheckpoints checkpoints, params string[] checkpointsNames)
        {
            var reachedCheckpoints = checkpointsNames.Where(checkpoints.IsCheckpointReached).ToArray();
            CollectionAssert.IsEmpty(
                reachedCheckpoints,
                $"Some checkpoints are reached: {string.Join(", ", reachedCheckpoints)}");
        }

        public static void CheckCheckpointsReached(this TestCheckpoints checkpoints, params string[] checkpointsNames)
        {
            var reachedCheckpoints = checkpointsNames.Where(checkpoints.IsCheckpointReached).ToArray();
            CollectionAssert.AreEquivalent(
                checkpointsNames,
                reachedCheckpoints,
                $"Reached checkpoints are not valid. Reached are: {string.Join(", ", reachedCheckpoints)}");
        }

        #endregion
    }
}

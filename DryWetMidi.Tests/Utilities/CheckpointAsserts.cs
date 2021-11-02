using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class CheckpointAsserts
    {
        #region Methods

        public static void CheckCheckpointNotReached(this TestCheckpoints checkpoints, string checkpointName) =>
            Assert.IsFalse(checkpoints.IsCheckpointReached(checkpointName), $"Checkpoint [{checkpointName}] is reached.");

        public static void CheckCheckpointReached(this TestCheckpoints checkpoints, string checkpointName) =>
            Assert.IsTrue(checkpoints.IsCheckpointReached(checkpointName), $"Checkpoint [{checkpointName}] is not reached.");

        #endregion
    }
}

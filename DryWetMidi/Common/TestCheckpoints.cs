using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class TestCheckpoints

    {
        #region Fields

        private readonly Dictionary<string, bool> _checkpointsReachedStates = new Dictionary<string, bool>();

        #endregion

        #region Methods

        public void SetCheckpointReached(string checkpointName)
        {
            _checkpointsReachedStates[checkpointName] = true;
        }

        public bool IsCheckpointReached(string checkpointName)
        {
            bool state;
            return _checkpointsReachedStates.TryGetValue(checkpointName, out state) && state;
        }

        #endregion
    }
}

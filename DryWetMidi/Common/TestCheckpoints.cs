using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class TestCheckpoints

    {
        #region Fields

        private readonly Dictionary<string, List<object>> _checkpointsReachedStates = new Dictionary<string, List<object>>();

        #endregion

        #region Methods

        public void SetCheckpointReached(string checkpointName)
        {
            SetCheckpointReached(checkpointName, null);
        }

        public void SetCheckpointReached(string checkpointName, object data)
        {
            List<object> dataList;
            if (!_checkpointsReachedStates.TryGetValue(checkpointName, out dataList))
                _checkpointsReachedStates.Add(checkpointName, dataList = new List<object>());

            dataList.Add(data);
        }

        public bool IsCheckpointReached(string checkpointName)
        {
            List<object> dataList;
            return _checkpointsReachedStates.TryGetValue(checkpointName, out dataList) && dataList.Any();
        }

        public ICollection<object> GetCheckpointDataList(string checkpointName)
        {
            List<object> dataList;
            return _checkpointsReachedStates.TryGetValue(checkpointName, out dataList)
                ? dataList
                : null;
        }

        #endregion
    }
}

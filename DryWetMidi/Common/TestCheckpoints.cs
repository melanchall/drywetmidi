using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class TestCheckpoints

    {
        #region Fields

        private readonly Dictionary<string, List<object?>> _checkpointsReachedStates = new Dictionary<string, List<object?>>();
        private readonly List<string> _errors = new List<string>();

        #endregion

        #region Methods

        public void SetErrorReached(string error)
        {
            _errors.Add(error);
        }

        public void SetCheckpointReached(string checkpointName)
        {
            SetCheckpointReached(checkpointName, null);
        }

        public void SetCheckpointReached(string checkpointName, object? data)
        {
            if (!_checkpointsReachedStates.TryGetValue(checkpointName, out var dataList))
                _checkpointsReachedStates.Add(checkpointName, dataList = new List<object?>());

            dataList.Add(data);
        }

        public bool IsCheckpointReached(string checkpointName)
        {
            return _checkpointsReachedStates.TryGetValue(checkpointName, out var dataList) && dataList.Any();
        }

        public ICollection<object?>? GetCheckpointDataList(string checkpointName)
        {
            return _checkpointsReachedStates.TryGetValue(checkpointName, out var dataList)
                ? dataList
                : null;
        }

        public ICollection<string> GetErrors()
        {
            return _errors;
        }

        #endregion
    }
}

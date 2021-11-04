using UnityEngine;
using Utilities.Enum;

namespace Utilities.Debugger
{
    [System.Serializable]
    public class LogSpace
    {
        [SerializeField] private LogSpaceEnum _logSpaceLogSpaceEnum;
        [SerializeField] private LogSpaceItem _logSpaceItem;

        public LogSpaceEnum LogSpaceEnum => _logSpaceLogSpaceEnum;
        public LogSpaceItem LogSpaceItem => _logSpaceItem;
    }
}
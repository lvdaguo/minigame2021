using UnityEngine;

namespace Utilities.ObjectPool
{
    [System.Serializable]
    public class PoolSetting
    {
        [SerializeField] private GameObject _prefab;
        [Min(0), SerializeField] private int _size;
        
        [SerializeField] private ResizeMode _resizeMode = ResizeMode.Async;
        
        [Min(1), SerializeField] private int _defaultWaitFrames = 1;
        [Min(1), SerializeField] private int _defaultObjectsPerWait = 5;
        
        public GameObject Prefab => _prefab;
        public int Size => _size;
        public ResizeMode ResizeMode => _resizeMode;
        /// <summary> 默认异步帧间隔 </summary>
        public int DefaultWaitFrames => _defaultWaitFrames;
        /// <summary> 默认每次异步间隔的加载/卸载个数 </summary>
        public int DefaultObjectsPerWait => _defaultObjectsPerWait;
    }
}
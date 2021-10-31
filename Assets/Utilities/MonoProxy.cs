using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utilities.ObjectPool;

namespace Utilities
{
    [Serializable]
    internal class UtilInitValues
    {
        [Header("[VolumeController]")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("[ObjectPool]")]
        [SerializeField] private ObjectPoolConfigSO _objectPoolConfigSO;
        
        public AudioMixer AudioMixer => _audioMixer;
        public ObjectPoolConfigSO ObjectPoolConfigSO => _objectPoolConfigSO;

        public string SaveFolder => Application.persistentDataPath;
    }
    
    /// <summary>
    /// 静态管理类的单例代理
    /// 使用SerializeField帮助管理类获取引用
    /// 同时作为静态管理类（非MonoBehaviour子类）的协程启动器
    /// </summary>
    public class MonoProxy : MonoBehaviour
    {
        /// <summary> 单例 </summary>
        public static MonoProxy Instance { get; private set; }

        [SerializeField] private UtilInitValues _utilInitValues;
        
        internal Action<UtilInitValues> InitDelegate = delegate { }; 

        public event Action UpdateEvent = delegate { };
        public event Action FixedUpdateEvent = delegate { };
        public event Action DestroyEvent = delegate { };

        /// <summary> 单例初始化 </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                AddToInitDelegate();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void AddToInitDelegate()
        {
            InitDelegate += SceneLoader.Init;
            InitDelegate += GCScheduler.Init;
            InitDelegate += GraphicOptions.Init;
            InitDelegate += VolumeController.Init;
            InitDelegate += OptionSaver.Init;
            InitDelegate += ObjectPool.ObjectPool.Init;
        }
        
        private void Start() => InitDelegate.Invoke(_utilInitValues);

        private void Update() => UpdateEvent.Invoke();

        private void FixedUpdate() => FixedUpdateEvent.Invoke();

        private void OnDestroy() => DestroyEvent.Invoke();
    }
}
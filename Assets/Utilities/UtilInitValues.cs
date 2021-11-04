using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utilities.Debugger;
using Utilities.Event;
using Utilities.Pool;

namespace Utilities
{
    [Serializable]
    internal class UtilInitValues
    {
        [Header("[VolumeController]")] [SerializeField]
        private AudioMixer _audioMixer;

        [Header("[ObjectPool]")] [SerializeField]
        private ObjectPoolConfigSO _objectPoolConfigSO;

        [Header("[Singletons]")] [SerializeField]
        private List<GameObject> _globalManagerPrefabList;

        [Header("[LogSystem]")] [SerializeField]
        private LogConfigSO _logConfigSO;

        [Header("[EventCenter]")] [SerializeField]
        private EventCenterConfigSO _eventCenterConfigSO;

        public AudioMixer AudioMixer => _audioMixer;
        public ObjectPoolConfigSO ObjectPoolConfigSO => _objectPoolConfigSO;
        public List<GameObject> GlobalManagerPrefabList => _globalManagerPrefabList;
        public LogConfigSO LogConfigSO => _logConfigSO;

        public EventCenterConfigSO EventCenterConfigSO => _eventCenterConfigSO;
        public string SaveFolder => Application.persistentDataPath;
    }
}
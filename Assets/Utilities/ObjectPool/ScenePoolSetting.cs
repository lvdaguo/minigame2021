﻿using UnityEngine;

namespace Utilities.ObjectPool
{
    [System.Serializable]
    public class ScenePoolSetting
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private PoolSettingListSO _poolSettingListSO;

        public string SceneName => _sceneName;
        public PoolSettingListSO PoolSettingListSO => _poolSettingListSO;
    }
}
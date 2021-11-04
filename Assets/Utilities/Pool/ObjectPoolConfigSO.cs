using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Pool
{
    [CreateAssetMenu(fileName = "ObjectPoolConfig", menuName = "Utilities/Object Pool Config")]
    public class ObjectPoolConfigSO : ScriptableObject
    {
        [SerializeField] private List<ScenePoolSetting> _sceneOverrideSettings;
        [SerializeField] private PoolSettingListSO _defaultPoolSettings;
        public List<ScenePoolSetting> SceneOverrideSettings => _sceneOverrideSettings;
        public PoolSettingListSO DefaultPoolSettings => _defaultPoolSettings;
    }
}
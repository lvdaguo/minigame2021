using System.Collections.Generic;
using UnityEngine;

namespace Utilities.ObjectPool
{
    [CreateAssetMenu(fileName = "PoolSettingList", menuName = "Utilities/Pool Setting List")]
    public class PoolSettingListSO : ScriptableObject
    {
        [SerializeField] private List<PoolSetting> _poolSettings;
        public List<PoolSetting> PoolSettings => _poolSettings;
    }
}
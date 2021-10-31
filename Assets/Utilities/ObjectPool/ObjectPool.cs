using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.DataStructures;

namespace Utilities.ObjectPool
{
    public static class ObjectPool
    {
        private static Dictionary<string, Dictionary<GameObject ,PoolSetting>> _sceneOverrideSettingMap;
        private static Dictionary<GameObject, PoolAgent> _poolsMapping;

        private static PoolAgent GetPoolAgent(GameObject prefab) => _poolsMapping[prefab];
        private static bool _preLoadAsyncDone;
        
        internal static void Init(UtilInitValues utilInitValues)
        {
            ObjectPoolConfigSO config = utilInitValues.ObjectPoolConfigSO;
            
            InitMembers();
            ListenToEvents();

            GameObject objectPool = GeneratePoolRootNode();
            AddSceneOverrideSettingMap(config.SceneOverrideSettings);
            GeneratePoolAgents(config.DefaultPoolSettings.PoolSettings, objectPool);
        }

        private static void InitMembers()
        {
            _sceneOverrideSettingMap = new Dictionary<string, Dictionary<GameObject, PoolSetting>>();
            _poolsMapping = new Dictionary<GameObject, PoolAgent>();
        }

        private static void ListenToEvents()
        {
            SceneLoader.PreLoad += OnPreLoad;
            SceneLoader.PreLoadAsync += OnPreLoadAsync;
            
            SceneLoader.PreLoadReadyConnect(() => _preLoadAsyncDone);
        }
        
        private static GameObject GeneratePoolRootNode()
        {
            GameObject objectPool = new GameObject {name = "ObjectPool"};
            Object.DontDestroyOnLoad(objectPool);
            return objectPool;
        }
        
        private static void AddSceneOverrideSettingMap(List<ScenePoolSetting> sceneOverrideSettings)
        {
            foreach (ScenePoolSetting scenePoolSetting in sceneOverrideSettings)
            {
                Dictionary<GameObject, PoolSetting> poolSettingMap = new Dictionary<GameObject, PoolSetting>();
                foreach (PoolSetting poolSetting in scenePoolSetting.PoolSettingListSO.PoolSettings)
                {
                    poolSettingMap.Add(poolSetting.Prefab, poolSetting);
                }
                _sceneOverrideSettingMap.Add(scenePoolSetting.SceneName, poolSettingMap);
            }
        }

        private static void GeneratePoolAgents(List<PoolSetting> poolSettings, GameObject objectPool)
        {
            foreach (PoolSetting setting in poolSettings)
            {
                GeneratePoolAgent(setting, objectPool);
            }
        }
        
        private static void GeneratePoolAgent(PoolSetting poolSetting, GameObject objectPool)
        {
            GameObject poolAgentGO = new GameObject {name = poolSetting.Prefab.name};
            poolAgentGO.transform.SetParent(objectPool.transform);
            
            PoolAgent poolAgent = poolAgentGO.AddComponent<PoolAgent>();
            poolAgent.Init(poolSetting);

            _poolsMapping.Add(poolSetting.Prefab, poolAgent);

            PreWarm(poolSetting);
        }

        private static void PreWarm(PoolSetting defaultPoolSetting)
        {
            PoolSetting preWarmPoolSetting = TryOverride(SceneLoader.ActiveScene, defaultPoolSetting);
            
            if (preWarmPoolSetting.ResizeMode == ResizeMode.Sync)
            {
                Resize(preWarmPoolSetting.Prefab, preWarmPoolSetting.Size);
            }
            else
            {
                ResizeAsync(preWarmPoolSetting.Prefab, preWarmPoolSetting.Size, 
                    preWarmPoolSetting.DefaultWaitFrames, preWarmPoolSetting.DefaultObjectsPerWait);
            }
        }

        private static void OnPreLoad(string sceneName)
        {
            foreach (PoolAgent poolAgent in _poolsMapping.Values)
            {
                PoolSetting poolSetting = TryOverride(sceneName, poolAgent.DefaultPoolSetting);
                if (poolSetting.ResizeMode == ResizeMode.Sync)
                {
                    Resize(poolSetting.Prefab, poolSetting.Size);
                }
            }
        }

        private static void OnPreLoadAsync(string sceneName)
        {
            MonoProxy.Instance.StartCoroutine(OnPreLoadAsyncCo(sceneName));
        }

        private static IEnumerator OnPreLoadAsyncCo(string sceneName)
        {
            _preLoadAsyncDone = false;
            foreach (PoolAgent poolAgent in _poolsMapping.Values)
            {
                PoolSetting poolSetting = TryOverride(sceneName, poolAgent.DefaultPoolSetting);
                if (poolSetting.ResizeMode == ResizeMode.Async)
                {
                    AsyncHandle asyncHandle = ResizeAsync(poolSetting.Prefab, poolSetting.Size, 
                        poolSetting.DefaultWaitFrames, poolSetting.DefaultObjectsPerWait);
                    yield return Wait.Until(() => asyncHandle.IsDone);
                }
            }
            _preLoadAsyncDone = true;
        }

        private static PoolSetting TryOverride(string sceneName, PoolSetting defaultPoolSetting)
        {
            if (_sceneOverrideSettingMap.TryGetValue(sceneName, 
                out Dictionary<GameObject, PoolSetting> prefabPoolSettingMap))
            {
                if (prefabPoolSettingMap.TryGetValue(defaultPoolSetting.Prefab, out PoolSetting overridePoolSetting))
                {
                    return overridePoolSetting;
                }
            }
            return defaultPoolSetting;
        }

        /// <summary> 获取物体 </summary>
        /// <param name="original"> 预制体名 </param>
        public static GameObject Spawn(GameObject original)
        {
            GameObject go = GetPoolAgent(original).BaseSpawn();
            go.SetActive(true);
            return go;
        }

        /// <summary> 获取物体并设定父物体（世界位置为默认位置+父物体位置） </summary>
        /// <param name="original"> 预制体名 </param>
        /// <param name="parent"> 父物体 </param>
        /// <param name="spawnInWorldSpace"> ，为true则为默认位置，为false物体位置为父物体位置+默认位置 </param>
        public static GameObject Spawn(GameObject original, Transform parent, bool spawnInWorldSpace = false)
        {
            GameObject go = GetPoolAgent(original).BaseSpawn();
            go.transform.SetParent(parent, spawnInWorldSpace);
            // SetActive放在最后，此时执行OnEnable，保证对象已经在新的parent下，而不是对象池结点
            go.SetActive(true);
            return go;
        }

        /// <summary> 获取物体并设定位置和旋转 </summary>
        /// <param name="original"> 预制体 </param>
        /// <param name="position"> 位置 </param>
        /// <param name="rotation"> 旋转 </param>
        public static GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation)
        {
            GameObject go = GetPoolAgent(original).BaseSpawn();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }
        
        /// <summary> 获取物体并设定位置，旋转和父物体（位置为世界坐标，不与父物体相关） </summary>
        /// <param name="original"> 预制体 </param>
        /// <param name="position"> 位置 </param>
        /// <param name="rotation"> 旋转 </param>
        /// <param name="parent"> 父物体 </param>
        public static GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject go = GetPoolAgent(original).BaseSpawn();
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.SetActive(true);
            return go;
        }

        /// <summary> 延迟返还物体 </summary>
        /// <param name="go"> 返还的游戏物体 </param>
        /// <param name="delay"> 返还的延迟时间 </param>
        public static void Return(GameObject go, float delay = 0f)
        {
            GetPoolAgent(go).BaseReturn(go, delay);
        }

        /// <summary> 调整大小 </summary>
        /// <param name="prefab"> 预制体 </param>
        /// <param name="newSize"> 新大小 </param>
        public static void Resize(GameObject prefab, int newSize)
        {
            GetPoolAgent(prefab).BaseResize(newSize);
        }

        /// <summary> 异步调整大小 </summary>
        /// <param name="prefab"> 预制体 </param>
        /// <param name="newSize"> 新大小 </param>
        /// <param name="waitFrames"> 等待间隔帧数 </param>
        /// <param name="objectsPerWait"> 每次等待个数 </param>
        /// <returns></returns>
        public static AsyncHandle ResizeAsync(GameObject prefab, int newSize, int waitFrames, int objectsPerWait)
        {
            return GetPoolAgent(prefab).BaseResizeAsync(newSize, waitFrames, objectsPerWait);
        }
    }
}
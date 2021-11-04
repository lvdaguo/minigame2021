using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utilities.DataStructures;
using Utilities.Debugger;
using Utilities.Enum;

namespace Utilities
{
    /// <summary>
    /// 场景加载器
    /// </summary>
    public static class SceneLoader
    {
        /// <summary> 进度条更新事件 </summary>
        public static event Action<float> ProgressUpdate = progress => Progress = progress;

        /// <summary> 是否正在加载 </summary>
        public static bool IsLoading { get; private set; }

        /// <summary> 加载进度 </summary>
        public static float Progress { get; private set; } = 1.0f;

        /// <summary> 当前场景 </summary>
        public static string ActiveScene => SceneManager.GetActiveScene().name;


        /// <summary> 预加载事件 </summary>
        public static event Action<string> PreSceneLoad = delegate { };

        /// <summary> 异步预加载事件 </summary>
        public static event Action<string> PreSceneLoadAsync = delegate { };

        /// <summary> 异步预加载完成判定 </summary>
        private static readonly HashSet<Func<bool>> _preLoadReady = new HashSet<Func<bool>>();

        /// <summary> 异步完成判定绑定 </summary>
        /// <param name="ready"> 判定函数 </param>
        public static void PreSceneLoadReadyConnect(Func<bool> ready)
        {
            _preLoadReady.Add(ready);
        }

        /// <summary> 异步完成判定解绑 </summary>
        /// <param name="ready"> 判定函数 </param>
        public static void PreSceneLoadReadyDisconnect(Func<bool> ready)
        {
            _preLoadReady.Remove(ready);
        }

        /// <summary> 预处理事件 </summary>
        public static event Action<string> AfterSceneLoad = delegate { };

        /// <summary> 异步预处理事件 </summary>
        public static event Action<string> AfterSceneLoadAsync = delegate { };

        /// <summary> 异步预处理完成判定 </summary>
        private static readonly HashSet<Func<bool>> _preProcReady = new HashSet<Func<bool>>();

        /// <summary> 异步完成判定绑定 </summary>
        /// <param name="ready"> 判定函数 </param>
        public static void AfterSceneLoadReadyConnect(Func<bool> ready)
        {
            _preProcReady.Add(ready);
        }

        /// <summary> 异步完成判定解绑 </summary>
        /// <param name="ready"> 判定函数 </param>
        public static void AfterSceneLoadReadyDisconnect(Func<bool> ready)
        {
            _preProcReady.Remove(ready);
        }

        /// <summary> 当前场景更改事件 </summary>
        public static event UnityAction<Scene, Scene> ActiveSceneChanged = delegate { };

        /// <summary> 场景加载完毕事件 </summary>
        public static event UnityAction<Scene, LoadSceneMode> SceneLoaded = delegate { };

        /// <summary> 场景卸载完毕事件 </summary>
        public static event UnityAction<Scene> SceneUnloaded = delegate { };

        // 生命周期顺序为
        // Awake -- OnEnable -- ActiveSceneChanged -- SceneLoaded
        // Start -- OnDisable -- OnDestroy -- SceneUnloaded

        /// <summary> 初始化 </summary>
        internal static void Init(UtilInitValues utilInitValues)
        {
            static void ChangeScene(Scene lhs, Scene rhs) => ActiveSceneChanged.Invoke(lhs, rhs);
            static void Loaded(Scene scene, LoadSceneMode mode) => SceneLoaded.Invoke(scene, mode);
            static void Unloaded(Scene scene) => SceneUnloaded.Invoke(scene);

            // 复用SceneManager的事件方法
            SceneManager.activeSceneChanged += ChangeScene;
            SceneManager.sceneLoaded += Loaded;
            SceneManager.sceneUnloaded += Unloaded;
        }

        /// <summary> 确保预加载全部完成 </summary>
        private static bool PreLoadFinished()
        {
            foreach (Func<bool> ready in _preLoadReady)
            {
                if (ready() == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary> 确保预处理全部完成 </summary>
        private static bool PreProcFinished()
        {
            foreach (Func<bool> ready in _preProcReady)
            {
                if (ready() == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary> 异步加载场景 </summary>
        /// <param name="sceneName"> 场景名 </param>
        /// <param name="reactTime"> 加载完毕后的延迟时间 </param>
        /// <param name="transitionSceneName"> 加载时的过渡场景名（使用非异步加载，为空时不使用过渡） </param>
        public static void LoadScene(string sceneName, float reactTime = 0f, string transitionSceneName = null)
        {
            if (IsLoading)
            {
                Log.PrintError("已在加载异步场景", LogSpaceEnum.SceneLoader);
                return;
            }
            MonoProxy.Instance.StartCoroutine(LoadSceneCo(sceneName, reactTime, transitionSceneName));
        }

        /// <summary> 异步加载目标场景实现 </summary>
        private static IEnumerator LoadSceneCo(string sceneName, float reactTime, string transitionSceneName)
        {
            IsLoading = true;
            if (transitionSceneName != null)
            {
                Log.Print("加载过渡场景: " + transitionSceneName, LogSpaceEnum.SceneLoader);
                SceneManager.LoadScene(transitionSceneName);
            }

            Log.Print("启动预加载", LogSpaceEnum.SceneLoader);
            PreSceneLoad(sceneName);
            if (_preLoadReady.Count > 0)
            {
                PreSceneLoadAsync(sceneName);
                while (PreLoadFinished() == false)
                {
                    yield return null;
                }
            }
            Log.Print("预加载完毕", LogSpaceEnum.SceneLoader);
            
            ProgressUpdate(0.0f);

            Log.Print("正式开始异步加载场景" + sceneName, LogSpaceEnum.SceneLoader);
            AsyncOperation info = SceneManager.LoadSceneAsync(sceneName);
            info.allowSceneActivation = false;

            while (info.progress < 0.9f)
            {
                ProgressUpdate(info.progress);
                yield return null;
            }
            
            ProgressUpdate(info.progress);

            Log.Print("启动后加载", LogSpaceEnum.SceneLoader);

            AfterSceneLoad(sceneName);
            if (_preProcReady.Count > 0)
            {
                AfterSceneLoadAsync(sceneName);
                while (PreProcFinished() == false)
                {
                    yield return null;
                }
            }
            
            Log.Print("后加载完毕", LogSpaceEnum.SceneLoader);

            ProgressUpdate(1.0f);
            yield return Wait.Seconds(reactTime);

            Log.Print("进入目标场景", LogSpaceEnum.SceneLoader);
            info.allowSceneActivation = true;

            IsLoading = false;
        }
    }
}
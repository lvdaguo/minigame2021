using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utilities.Enum;
using Object = UnityEngine.Object;
using static Utilities.StaticMethodClass.RichText;

namespace Utilities.Debugger
{ 
    public static class Log
    {
#if UNITY_EDITOR
        private static readonly ILogger _logger = Debug.unityLogger;
        private static Dictionary<LogSpaceEnum, LogSpaceItem> _logSpaceItemMap;
        private static Dictionary<LogSpaceEnum, string> _logSpaceStyleMap;
        private static LogConfigSO _logConfigSO;

        // 需要在Init前初始化， 以防Awake有用户发送Log请求时，queue还未初始化
        private static readonly Queue<WorkItem> _requestQueue = new Queue<WorkItem>();
        private static Queue<WorkItem> _record;
#endif

        internal static void Init(UtilInitValues utilInitValues)
        {
#if UNITY_EDITOR
            _logConfigSO = utilInitValues.LogConfigSO;
            InitMembers();
            GetMap();
            ListenToEvents();
            MonoProxy.Instance.StartCoroutine(WorkFlowCo());
#endif
        }

#if UNITY_EDITOR
        private static void InitMembers()
        {
            _logSpaceItemMap = new Dictionary<LogSpaceEnum, LogSpaceItem>();
            _logSpaceStyleMap = new Dictionary<LogSpaceEnum, string>();
            _record = new Queue<WorkItem>();
        }

        private static void GetItemMap()
        {
            _logSpaceItemMap.Clear();
            foreach (LogSpace logSpace in _logConfigSO.LogSpaces)
            {
                _logSpaceItemMap.Add(logSpace.LogSpaceEnum, logSpace.LogSpaceItem);
            }
            if (_logSpaceItemMap.Count != _logConfigSO.LogSpaces.Length)
            {
                Debug.LogError("配置文件中的日志空间含有重复");
            }
        }

        private static void GetStyleMap()
        {
            _logSpaceStyleMap.Clear();
            foreach (KeyValuePair<LogSpaceEnum, LogSpaceItem> pair in _logSpaceItemMap)
            {
                LogSpaceEnum logSpaceEnum = pair.Key;
                LogSpaceItem logSpaceItem = pair.Value;
                string logSpaceStyle = GetLogSpaceStyle(logSpaceEnum.ToString(), logSpaceItem.Color);
                _logSpaceStyleMap.Add(logSpaceEnum, logSpaceStyle);
            }
        }
        
        private static string GetLogSpaceStyle(string logSpace, Color color)
        {
            string logSpaceStyle = logSpace;
            if (_logConfigSO.IsBold)
            {
                logSpaceStyle = Bold(logSpaceStyle);
            }
            if (_logConfigSO.IsItalic)
            {
                logSpaceStyle = Italic(logSpaceStyle);
            }
            logSpaceStyle = Brackets(logSpaceStyle);
            logSpaceStyle = Size(logSpaceStyle, _logConfigSO.Size);
            logSpaceStyle = Color(logSpaceStyle, color);
            return logSpaceStyle;
        }

        private static void GetMap()
        {
            GetItemMap();
            // 获取顺序在ItemMap之后，保证StyleMap的Key都和ItemMap一致
            GetStyleMap();
        }

        private static void ListenToEvents()
        {
            _logConfigSO.ValueChange += OnValueChange;
        }

        private static void OnValueChange()
        {
            GetMap();
            ClearConsole();
            if (_logConfigSO.Activated)
            {
                ShowConsole();
            }
        }

        private static void ShowConsole()
        {
            foreach (WorkItem logItem in _record)
            {
                LogBase(logItem, false);
            }
        }

        private static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
            clearConsoleMethod?.Invoke(new object(), null);
        }

        private static IEnumerator WorkFlowCo()
        {
            while (true)
            {
                if (_requestQueue.Count > 0 && _logConfigSO.Activated)
                {
                    LogBase(_requestQueue.Dequeue());
                }
                yield return null;
            }
        }

        private static void SendWorkRequest(LogType logType, LogSpaceEnum logSpaceEnum, object message,
            Object gameObject)
        {
            _requestQueue.Enqueue(new WorkItem(logType, logSpaceEnum, message, gameObject));
        }

        private static void LogBase(WorkItem workItem, bool record = true)
        {
            if (_logSpaceItemMap.TryGetValue(workItem.LogSpaceEnum, out LogSpaceItem logSpaceItem) == false)
            {
                Debug.LogError("没有在Config中加入日志空间: " + workItem.LogSpaceEnum);
                return;
            }
            if (record)
            {
                if (_record.Count == _logConfigSO.LogBufferSize)
                {
                    Debug.Log("日志缓存已满，旧的日志记录将丢失");
                    _record.Dequeue();
                }
                _record.Enqueue(workItem);
            }
            if (logSpaceItem.Enabled)
            {
                _logger.Log(workItem.LogType, _logSpaceStyleMap[workItem.LogSpaceEnum], workItem.Content,
                    workItem.GameObject);
            }
        }
#endif
        
        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"> 消息 </param>
        /// <param name="logSpaceEnum"> 日志空间 </param>
        /// <param name="gameObject"> 发送者 </param>
        public static void Print(object message, LogSpaceEnum logSpaceEnum, Object gameObject = null)
        {
#if UNITY_EDITOR
            SendWorkRequest(LogType.Log, logSpaceEnum, message, gameObject);
#endif
        }

        /// <summary>
        /// 打印警告日志
        /// </summary>
        /// <param name="message"> 警告消息 </param>
        /// <param name="logSpaceEnum"> 日志空间 </param>
        /// <param name="gameObject"> 发送者 </param>
        public static void PrintWarning(object message, LogSpaceEnum logSpaceEnum, Object gameObject = null)
        {
#if UNITY_EDITOR
            SendWorkRequest(LogType.Warning, logSpaceEnum, message, gameObject);
#endif
        }

        /// <summary>
        /// 打印错误日志
        /// </summary>
        /// <param name="message"> 错误消息 </param>
        /// <param name="logSpaceEnum"> 日志空间 </param>
        /// <param name="gameObject"> 发送者 </param>
        public static void PrintError(object message, LogSpaceEnum logSpaceEnum, Object gameObject = null)
        {
#if UNITY_EDITOR
            SendWorkRequest(LogType.Error, logSpaceEnum, message, gameObject);
#endif
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public static void Break()
        {
#if UNITY_EDITOR
            Debug.Break();
#endif
        }

        /// <summary>
        /// 绘制线段
        /// </summary>
        /// <param name="start"> 起点 </param>
        /// <param name="end"> 终点 </param>
        /// <param name="color"> 颜色 </param>
        /// <param name="duration"> 持续时间 </param>
        /// <param name="depthTest"> 是否被物体遮挡 </param>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = true)
        {
#if UNITY_EDITOR
            Debug.DrawLine(start, end, color, duration, depthTest);
#endif
        }

        /// <summary>
        /// 绘制射线
        /// </summary>
        /// <param name="start"> 起点 </param>
        /// <param name="dir"> 方向 </param>
        /// <param name="color"> 颜色 </param>
        /// <param name="duration"> 持续时间 </param>
        /// <param name="depthTest"> 是否被物体遮挡 </param>
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0f, bool depthTest = true)
        {
#if UNITY_EDITOR
            Debug.DrawRay(start, dir, color, duration, depthTest);
#endif
        }
        
#if UNITY_EDITOR
        private readonly struct WorkItem
        {
            public LogType LogType { get; }
            public LogSpaceEnum LogSpaceEnum { get; }
            public object Content { get; }

            public Object GameObject { get; }
        
            public WorkItem(LogType logType, LogSpaceEnum logSpaceEnum, object message, Object gameObject)
            {
                LogType = logType;
                Content = message;
                LogSpaceEnum = logSpaceEnum;
                GameObject = gameObject;
            }
        }
#endif
    }
}
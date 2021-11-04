using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Debugger;
using Utilities.Enum;

namespace Utilities.Event
{
    using CallBack = Action<EventArguments>;
    using CallBackMap = Dictionary<string, Action<EventArguments>>;
    using EventSpaceMap = Dictionary<EventSpaceEnum, Dictionary<string, Action<EventArguments>>>;

    public static class EventCenter
    {
        private static EventSpaceMap _eventSpaceMap;

        // 同步所有的订阅广播操作, 防止初始化前用户发起请求
        private static readonly Queue<WorkItem> _workQueue = new Queue<WorkItem>();

        internal static void Init(UtilInitValues utilInitValues)
        {
            InitMembers(utilInitValues.EventCenterConfigSO);

            MonoProxy.Instance.StartCoroutine(WorkFlowCo());
        }

        private static void InitMembers(EventCenterConfigSO config)
        {
            _eventSpaceMap = new EventSpaceMap();
            foreach (EventSpaceEnum eventSpaceEnum in config.EventSpaceEnums)
            {
                _eventSpaceMap.Add(eventSpaceEnum, new Dictionary<string, CallBack>());
            }

            if (config.EventSpaceEnums.Count != _eventSpaceMap.Count)
            {
                Log.PrintError("配置文件中的事件空间含有重复", LogSpaceEnum.EventCenter);
            }
        }

        private static IEnumerator WorkFlowCo()
        {
            while (true)
            {
                if (_workQueue.Count > 0)
                {
                    HandleWorkItem(_workQueue.Dequeue());
                }
                yield return null;
            }
        }

        private static void HandleWorkItem(WorkItem workItem)
        {
            if (workItem is AddListenerWorkItem addListenerWorkItem)
            {
                AddListenerBase(addListenerWorkItem);
            }
            else if (workItem is RemoveListenerWorkItem removeListenerWorkItem)
            {
                RemoveListenerBase(removeListenerWorkItem);
            }
            else if (workItem is BroadcastWorkItem broadcastWorkItem)
            {
                BroadcastBase(broadcastWorkItem);
            }
        }

        private static CallBackMap GetCallBackMap(EventSpaceEnum eventSpaceEnum)
        {
            if (_eventSpaceMap.TryGetValue(eventSpaceEnum, out CallBackMap callBackMap) == false)
            {
                Log.PrintError("不存在事件空间: " + eventSpaceEnum, LogSpaceEnum.EventCenter);
            }
            return callBackMap;
        }

        private static CallBack GetCallBack(EventSpaceEnum eventSpaceEnum, string eventName)
        {
            CallBackMap callBackMap = GetCallBackMap(eventSpaceEnum);
            if (callBackMap == null)
            {
                return null;
            }

            if (callBackMap.TryGetValue(eventName, out CallBack callBack) == false)
            {
                Log.PrintError("不存在事件: " + eventName, LogSpaceEnum.EventCenter);
            }
            return callBack;
        }

        private static void AddListenerBase(AddListenerWorkItem item)
        {
            CallBackMap callBackMap = GetCallBackMap(item.EventSpaceEnum);
            if (callBackMap == null)
            {
                Log.PrintError("添加事件时出错", LogSpaceEnum.EventCenter);
                return;
            }
            
            if (callBackMap.ContainsKey(item.EventName))
            {
                callBackMap[item.EventName] += item.CallBack;
            }
            else
            {
                callBackMap.Add(item.EventName, item.CallBack);
                Log.Print("已在" + item.EventSpaceEnum + "事件空间中创建事件: " + item.EventName, LogSpaceEnum.EventCenter);
            }

            Log.Print("已向" + item.EventSpaceEnum + "事件空间中的" + item.EventName + "事件添加响应动作", LogSpaceEnum.EventCenter);
        }
        
        private static void RemoveListenerBase(RemoveListenerWorkItem item)
        {
            CallBackMap callBackMap = GetCallBackMap(item.EventSpaceEnum);
            if (callBackMap == null)
            {
                Log.PrintError("移除事件时出错", LogSpaceEnum.EventCenter);
                return;
            }
            
            if (callBackMap.ContainsKey(item.EventName))
            {
                callBackMap[item.EventName] -= item.CallBack;

                Log.Print("已从" + item.EventSpaceEnum + "事件空间中的" + item.EventName + "事件移除响应动作", LogSpaceEnum.EventCenter);

                if (callBackMap[item.EventName] == null)
                {
                    callBackMap.Remove(item.EventName);
                    Log.Print("已在" + item.EventSpaceEnum + "事件空间中移除事件: " + item.EventName, LogSpaceEnum.EventCenter);
                }
            }
            else
            {
                Log.PrintError("移除事件时出错！" + item.EventSpaceEnum + "事件空间中不存在事件: " + item.EventName, LogSpaceEnum.EventCenter);
            }
        }
        
        private static void BroadcastBase(BroadcastWorkItem item)
        {
            CallBack callBack = GetCallBack(item.EventSpaceEnum, item.EventName);
            if (callBack == null)
            {
                Log.PrintError("广播事件时出错", LogSpaceEnum.EventCenter);
                return;
            }
            Log.Print("在" + item.EventSpaceEnum + "事件空间中广播事件: " + item.EventName, LogSpaceEnum.EventCenter);
            callBack.Invoke(item.EventArguments);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventSpaceEnum"> 事件空间 </param>
        /// <param name="eventName"> 事件名 </param>
        /// <param name="action"> 响应动作 </param>
        public static void AddListener(EventSpaceEnum eventSpaceEnum, string eventName, CallBack action)
        {
            _workQueue.Enqueue(new AddListenerWorkItem(eventSpaceEnum, eventName, action));
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="eventDef"> 事件 </param>
        /// <param name="action"> 响应动作 </param>
        public static void AddListener(EventDef eventDef, CallBack action)
        {
            AddListener(eventDef.EventSpaceEnumEnum, eventDef.EventName, action);
        }

        /// <summary>
        /// 撤销事件
        /// </summary>
        /// <param name="eventSpaceEnum"> 事件空间 </param>
        /// <param name="eventName"> 事件名 </param>
        /// <param name="action"> 响应动作 </param>
        public static void RemoveListener(EventSpaceEnum eventSpaceEnum, string eventName, CallBack action)
        {
            _workQueue.Enqueue(new RemoveListenerWorkItem(eventSpaceEnum, eventName, action));
        }

        /// <summary>
        /// 撤销事件
        /// </summary>
        /// <param name="eventDef"> 事件 </param>
        /// <param name="action"> 响应动作 </param>
        public static void RemoveListener(EventDef eventDef, CallBack action)
        {
            RemoveListener(eventDef.EventSpaceEnumEnum, eventDef.EventName, action);
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        /// <param name="eventSpaceEnum"> 事件空间 </param>
        /// <param name="eventName"> 事件名 </param>
        /// <param name="eventArguments"> 事件参数 </param>
        public static void Broadcast(EventSpaceEnum eventSpaceEnum, string eventName, EventArguments eventArguments = null)
        {
            _workQueue.Enqueue(new BroadcastWorkItem(eventSpaceEnum, eventName, eventArguments));
        }

        /// <summary>
        /// 广播事件
        /// </summary>
        /// <param name="eventDef"> 事件 </param>
        /// <param name="eventArguments"> 事件参数 </param>
        public static void Broadcast(EventDef eventDef, EventArguments eventArguments = null)
        {
            Broadcast(eventDef.EventSpaceEnumEnum, eventDef.EventName, eventArguments);
        }

        private abstract class WorkItem
        {
            public EventSpaceEnum EventSpaceEnum { get; }
            public string EventName { get; }

            protected WorkItem(EventSpaceEnum eventSpaceEnum, string eventName)
            {
                EventSpaceEnum = eventSpaceEnum;
                EventName = eventName;
            }
        }

        private class BroadcastWorkItem : WorkItem
        {
            public EventArguments EventArguments { get; }

            public BroadcastWorkItem(EventSpaceEnum eventSpaceEnum, string eventName, EventArguments eventArguments)
                : base(eventSpaceEnum, eventName)
            {
                EventArguments = eventArguments;
            }
        }

        private class AddListenerWorkItem : WorkItem
        {
            public CallBack CallBack { get; }

            public AddListenerWorkItem(EventSpaceEnum eventSpaceEnum, string eventName, CallBack action)
                : base(eventSpaceEnum, eventName)
            {
                CallBack = action;
            }
        }

        private class RemoveListenerWorkItem : WorkItem
        {
            public CallBack CallBack { get; }

            public RemoveListenerWorkItem(EventSpaceEnum eventSpaceEnum, string eventName, CallBack action)
                : base(eventSpaceEnum, eventName)
            {
                CallBack = action;
            }
        }
    }
}
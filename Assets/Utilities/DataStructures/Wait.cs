﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.DataStructures
{
    /// <summary>
    /// Wait（让出指令）缓存
    /// 对Wait类对象，使用池化或缓存
    /// 降低垃圾回收压力，提高性能
    /// </summary>
    public static class Wait
    {
        /// <summary> 默认池或缓存大小 </summary>
        private const int DefaultSize = 10;

        /// <summary> 禁用等待秒池 </summary>
        private static readonly bool _disableSecondsRealTimePool = true;

        /// <summary> 等待固定帧 </summary>
        private static WaitForFixedUpdate _fixedUpdate;

        /// <summary> 等待帧结束 </summary>
        private static WaitForEndOfFrame _endOfFrame;
        
        /// <summary> 等待秒池 </summary>
        private static readonly Stack<WaitForSecondsRealtimeCustom> _secondsRealtimePool;
        
        /// <summary> 等待帧池 </summary>
        private static readonly Stack<WaitForFrames> _framesPool;

        /// <summary> 等待直到条件池 </summary>
        private static readonly Stack<WaitUntilCustom> _untilPool;

        /// <summary> 等待当条件池 </summary>
        private static readonly Stack<WaitWhileCustom> _whilePool;
        
        /// <summary> 等待秒缓存，固定大小，根据使用频率保留最有可能使用的WaitForSeconds </summary>
        private static readonly LFUCache<float, WaitForSeconds> _secondsCache;
        
        /// <summary> 静态初始化 </summary>
        // 在创建第一个实例或引用任何静态成员之前，将自动调用静态构造函数。
        static Wait()
        {
            // 初始化Pool
            if (_disableSecondsRealTimePool == false)
            {
                _secondsRealtimePool = new Stack<WaitForSecondsRealtimeCustom>(DefaultSize);
                for (int i = 0; i < DefaultSize; ++i)
                {
                    _secondsRealtimePool.Push(new WaitForSecondsRealtimeCustom());
                }
            }

            _framesPool = new Stack<WaitForFrames>();
            _untilPool = new Stack<WaitUntilCustom>();
            _whilePool = new Stack<WaitWhileCustom>();

            // 初始化Cache
            _secondsCache = new LFUCache<float, WaitForSeconds>(DefaultSize);
        }
        
        /// <summary> 等待物理帧 </summary>
        public static WaitForFixedUpdate FixedUpdate()
        {
            return _fixedUpdate ??= new WaitForFixedUpdate();
        }
        
        /// <summary> 等待帧结束 </summary>
        public static WaitForEndOfFrame EndOfFrame()
        {
            // return _endOfFrame = _endOfFrame ?? new WaitForEndOfFrame();
            return _endOfFrame ??= new WaitForEndOfFrame();
        }
        
        /// <summary> 等待秒（不受TimeScale影响） </summary>
        public static WaitForSecondsRealtimeCustom SecondsRealtime(float time)
        {
            // 若池被禁用依然使用此方法，会因为pool没有初始化而抛出异常
            WaitForSecondsRealtimeCustom wait = _secondsRealtimePool.Count > 0 ? _secondsRealtimePool.Pop() : new WaitForSecondsRealtimeCustom();
            wait.Reset(time);
            return wait;
        }

        /// <summary> 等待帧 </summary>
        public static WaitForFrames Frames(int frames)
        {
            WaitForFrames wait = _framesPool.Count > 0 ? _framesPool.Pop() : new WaitForFrames();
            wait.Reset(frames);
            return wait;
        }
        
        /// <summary> 等待秒（受TimeScale影响） </summary>
        public static WaitForSeconds Seconds(float time)
        {
            if (_secondsCache.TryGetValue(time, out WaitForSeconds wait) == false)
            {
                wait = new WaitForSeconds(time);
                _secondsCache.Add(time, wait);
            }
            return wait;
        }

        /// <summary> 等待直到条件成立 </summary>
        public static WaitUntilCustom Until(Func<bool> predicate)
        {
            WaitUntilCustom wait = _untilPool.Count > 0 ? _untilPool.Pop() : new WaitUntilCustom();
            wait.Reset(predicate);
            return wait;
        }

        /// <summary> 等待当条件成立 </summary>
        public static WaitWhileCustom While(Func<bool> predicate)
        {
            WaitWhileCustom wait = _whilePool.Count > 0 ? _whilePool.Pop() : new WaitWhileCustom();
            wait.Reset(predicate);
            return wait;
        }
        
        /// <summary> 归还等待秒（内部使用） </summary>
        private static void Return(WaitForSecondsRealtimeCustom wait)
        {
            _secondsRealtimePool.Push(wait);
        }
        
        /// <summary> 归还等待帧（内部使用） </summary>
        private static void Return(WaitForFrames wait)
        {
            _framesPool.Push(wait);
        }

        /// <summary> 归还等待直到条件成立（内部使用） </summary>
        private static void Return(WaitUntilCustom wait)
        {
            _untilPool.Push(wait);
        }

        /// <summary> 归还等待当条件成立（内部使用） </summary>
        private static void Return(WaitWhileCustom wait)
        {
            _whilePool.Push(wait);
        }
        
        /// <summary>
        /// 延迟执行某动作
        /// </summary>
        /// <param name="act"> 动作 </param>
        /// <param name="time"> 延迟时间 </param>
        public static void Delayed(Action act, float time)
        {
            MonoProxy.Instance.StartCoroutine(DelayedCo(act, time));
        }

        /// <summary> 延迟执行某动作实现 </summary>
        private static IEnumerator DelayedCo(Action act, float time)
        {
            yield return Seconds(time);
            act.Invoke();
        }
        
        /// <summary>
        /// 继承自定义让出指令，实现可重用WaitForSecondsRealtime
        /// </summary>
        public sealed class WaitForSecondsRealtimeCustom : CustomYieldInstruction
        {
            /// <summary> 等待结束时间 </summary>
            private float _targetTime;

            /// <summary> 重用等待对象 </summary>
            public void Reset(float time)
            {
                _targetTime = Time.realtimeSinceStartup + time;
            }

            /// <summary> 重写YieldInstruction属性，给协程提供终止条件 </summary>
            public override bool keepWaiting
            {
                get
                {
                    bool keepWait = Time.realtimeSinceStartup < _targetTime;
                    if (keepWait == false)
                    {
                        // 将自己归还池 
                        Return(this);
                    }
                    return keepWait;
                }
            }
        }
        
        /// <summary>
        /// 继承自定义让出指令，实现可重用的等待固定帧类
        /// </summary>
        public sealed class WaitForFrames : CustomYieldInstruction
        {
            /// <summary> 剩余帧 </summary>
            private int _remainFrames;

            /// <summary> 计数器 </summary>
            private int _count;

            /// <summary> 重用等待对象 </summary>
            public void Reset(int frames)
            {
#if UNITY_EDITOR
                if (frames <= 0)
                {
                    Debug.LogError("参数错误，等待帧数为负数或0!");
                }
#endif
                _count = 0;
                _remainFrames = frames;
            }
        
            /// <summary> 重写YieldInstruction属性，给协程提供终止条件 </summary>
            public override bool keepWaiting
            {
                get
                {
                    bool keepWait = _count < _remainFrames;
                    _count++;
                    if (keepWait == false)
                    {
                        Return(this);
                    }
                    return keepWait;
                }
            }
        }
        
        
        /// <summary>
        /// 继承自定义让出指令，实现可重用的等待条件
        /// </summary>
        public sealed class WaitUntilCustom : CustomYieldInstruction
        {
            private Func<bool> _predicate;

            /// <summary> 重用等待对象 </summary>
            public void Reset(Func<bool> predicate)
            {
#if UNITY_EDITOR
                if (predicate == null)
                {
                    Debug.LogError("错误，条件为空");
                }
#endif
                _predicate = predicate;
            }

            /// <summary> 重写YieldInstruction属性，给协程提供终止条件 </summary>
            public override bool keepWaiting
            {
                get
                {
                    bool keepWait = _predicate.Invoke() == false;
                    if (keepWait == false)
                    {
                        Return(this);
                    }
                    return keepWait;
                }
            }
        }
        
        /// <summary>
        /// 继承自定义让出指令，实现可重用的等待条件
        /// </summary>
        public sealed class WaitWhileCustom : CustomYieldInstruction
        {
            private Func<bool> _predicate;
            
            /// <summary> 重用等待对象 </summary>
            public void Reset(Func<bool> predicate)
            {
#if UNITY_EDITOR
                if (predicate == null)
                {
                    Debug.LogError("错误，条件为空");
                }
#endif
                _predicate = predicate;
            }

            /// <summary> 重写YieldInstruction属性，给协程提供终止条件 </summary>
            public override bool keepWaiting
            {
                get
                {
                    bool keepWait = _predicate.Invoke() == false;
                    if (keepWait == false)
                    {
                        Return(this);
                    }
                    return keepWait;
                }
            }
        }
    }
}
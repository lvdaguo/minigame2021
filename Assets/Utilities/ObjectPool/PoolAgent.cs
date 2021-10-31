using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.DataStructures;

namespace Utilities.ObjectPool
{
    /// <summary>
    /// 对象池代理，自动挂载在GameObject上运行
    /// </summary>
    public sealed class PoolAgent : MonoBehaviour
    {
        private PoolSetting _poolSetting;
        internal PoolSetting DefaultPoolSetting => _poolSetting;
        
        private Vector3 _defaultPosition;
        private Quaternion _defaultRotation;

        private bool _inAsyncOperation;

        public event Action AsyncDoneEvent = delegate { };
        
        private Transform _poolTransform;

        private Stack<GameObject> _readyObjects;
        private Stack<GameObject> _appliedObjects;
        private Stack<GameObject> _returningObjects;

#if UNITY_EDITOR
        private FixedPrefixText _fixedPrefixText;
#endif
        internal void Init(PoolSetting poolSetting)
        {
            _poolSetting = poolSetting;
            InitMembers();
            GeneratePoolSubRootNode();
        }
        
        private void InitMembers()
        {
            _readyObjects = new Stack<GameObject>();
            _appliedObjects = new Stack<GameObject>();
            _returningObjects = new Stack<GameObject>();
            
#if UNITY_EDITOR
            _fixedPrefixText = new FixedPrefixText(_poolSetting.Prefab.name + " ");
#endif
            
            _inAsyncOperation = false;
            
            _defaultPosition = _poolSetting.Prefab.transform.position;
            _defaultRotation = _poolSetting.Prefab.transform.rotation;
            
            _poolTransform = transform;
        }

        private void GeneratePoolSubRootNode()
        {
            _poolTransform.gameObject.name = _poolSetting.Prefab.name + "Pool";
            // SetActive(false)后Apply对象时不会运行Awake、OnEnable
            _poolSetting.Prefab.SetActive(false);
        }

        /// <summary> 游戏结束时，将prefab设为true </summary>
        private void OnDestroy()
        {
            _poolSetting.Prefab.SetActive(true);
        }

        private GameObject Apply()
        {
            GameObject go = Instantiate(_poolSetting.Prefab, _defaultPosition, _defaultRotation, _poolTransform);
            _appliedObjects.Push(go);
            
#if UNITY_EDITOR
            SetNameWithIndex(go, _appliedObjects.Count - 1);
#endif
            
            return go;
        }
        
#if UNITY_EDITOR
        private void SetNameWithIndex(GameObject go, int index)
        {
            _fixedPrefixText.ChangeContent(index.ToString());
            go.name = _fixedPrefixText.ToString();
        }
#endif

        private bool NeedToExtend(int newSize) => _appliedObjects.Count < newSize;
        private bool NeedToShrink(int newSize) => _appliedObjects.Count > newSize;
        
        private void Extend() => _readyObjects.Push(Apply());
        private void Shrink() => Destroy(_appliedObjects.Pop());

        internal void BaseResize(int newSize)
        {
            while (NeedToExtend(newSize))
            {
                Extend();
            }
            while (NeedToShrink(newSize))
            {
                Shrink();
            }
        }

        private IEnumerator BaseResizeCo(int newSize, int asyncFrames, int objectsPerSlice)
        {
            _inAsyncOperation = true;
           
            while (NeedToExtend(newSize))
            {
                for (int i = 1; i <= objectsPerSlice && _appliedObjects.Count < newSize; ++i)
                {
                    Extend();           
                }
                yield return Wait.Frames(asyncFrames);
            }

            while (NeedToShrink(newSize))
            {
                for (int i = 1; i <= objectsPerSlice && _appliedObjects.Count > newSize; ++i)
                {
                    Shrink();
                }
                yield return Wait.Frames(asyncFrames);
            }

            AsyncDoneEvent.Invoke();
            _inAsyncOperation = false;
        }

        internal AsyncHandle BaseResizeAsync(int newSize, int waitFrames, int objectsPerWait)
        {
#if UNITY_EDITOR
            if (_inAsyncOperation)
            {
                Debug.LogWarning("已有异步操作执行，请等待其完成后再进行异步操作请求");
                return null;
            }
#endif
            AsyncHandle ah = new AsyncHandle(this);
            StartCoroutine(BaseResizeCo(newSize, waitFrames, objectsPerWait));
            return ah;
        }

        internal GameObject BaseSpawn()
        {
            // 不足，申请新的
            // 即便正在异步加载，也重新申请新的
            return _readyObjects.Count == 0 ? Apply() : _readyObjects.Pop();
        }

        private IEnumerator BaseReturnCo(GameObject go, float delay)
        {
            yield return Wait.Seconds(delay);
            // 加入缓冲栈
            _returningObjects.Push(go);
        }

        internal void BaseReturn(GameObject go, float delay)
        {
            if (delay == 0)
            {
                _returningObjects.Push(go);
            }
            else
            {
                StartCoroutine(BaseReturnCo(go, delay));
            }
        }

        /// <summary> 在Update结束后，渲染前执行真正的Return操作 </summary>
        private void LateUpdate()
        {
            // 清空缓冲栈
            while (_returningObjects.Count > 0)
            {
                GameObject go = _returningObjects.Pop();
                go.SetActive(false);
                go.transform.SetPositionAndRotation(_defaultPosition, _defaultRotation);
                go.transform.SetParent(_poolTransform);

                _readyObjects.Push(go);
            }
        }
    }
}


namespace Utilities.Pool
{
    /// <summary>
    /// 异步操作句柄
    /// 对象池使用
    /// </summary>
    public sealed class AsyncHandle
    {
        /// <summary> 调用者的引用 </summary>
        private readonly PoolAgent _poolAgent;

        public AsyncHandle(PoolAgent poolAgent)
        {
            IsDone = false;
            _poolAgent = poolAgent;
            _poolAgent.AsyncDoneEvent += OnAsyncDone;
        }

        public bool IsDone { get; private set; }

        private void OnAsyncDone()
        {
            IsDone = true;
            _poolAgent.AsyncDoneEvent -= OnAsyncDone;
        }
    }
}
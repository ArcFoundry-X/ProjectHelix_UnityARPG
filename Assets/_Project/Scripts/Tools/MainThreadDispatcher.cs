using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 主线程函数回调函数
/// 解决子线程发起事件，无法调用部分Unity API问题，比如无法修改UI
/// </summary>
public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher _instance;

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[MainThreadDispatcher]");
                _instance = go.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 高优先级（UI等）
    private readonly ConcurrentQueue<Action> _highPriorityQueue = new ConcurrentQueue<Action>();

    // 普通优先级（网络等）
    private readonly ConcurrentQueue<Action> _normalQueue = new ConcurrentQueue<Action>();

    // 每帧最大执行时间（秒）
    private float maxTimePerFrame = 0.005f; // 5ms

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // 🔹 基础执行
    // =========================

    public void Execute(Action action, bool highPriority = false)
    {
        if (action == null) return;

        if (highPriority)
            _highPriorityQueue.Enqueue(action);
        else
            _normalQueue.Enqueue(action);
    }

    // =========================
    // 🔹 带返回值
    // =========================

    public Task<T> ExecuteAsync<T>(Func<T> func, bool highPriority = false)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        Execute(() =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        }, highPriority);

        return tcs.Task;
    }

    // =========================
    // 🔹 支持 async 方法
    // =========================

    public Task ExecuteAsync(Func<Task> func, bool highPriority = false)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Execute(async () =>
        {
            try
            {
                await func();
                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        }, highPriority);

        return tcs.Task;
    }

    // =========================
    // 🔹 主线程执行
    // =========================

    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        // 先执行高优先级
        while (_highPriorityQueue.TryDequeue(out var action))
        {
            SafeInvoke(action);

            if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                return;
        }

        // 再执行普通任务
        while (_normalQueue.TryDequeue(out var action))
        {
            SafeInvoke(action);

            if (Time.realtimeSinceStartup - startTime > maxTimePerFrame)
                return;
        }
    }

    private void SafeInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YFramework.ResKit
{
    /// <summary>
    /// 热更新抽象接口
    ///
    /// 无论底层是 YooAsset Package 还是 Addressables RemoteCatalog，
    /// 调用方看到的都是这套统一流程。
    /// </summary>
    public interface IHotUpdateProvider
    {
        // ------------------------------------------------------------------
        // 事件
        // ------------------------------------------------------------------

        /// <summary>下载进度：(归一化[0-1], 已下载字节, 总字节)</summary>
        event Action<float, long, long> OnProgress;

        /// <summary>单个文件错误：(文件名, 错误信息)</summary>
        event Action<string, string> OnFileError;

        /// <summary>整体流程错误（可重试）</summary>
        event Action<string> OnError;

        /// <summary>热更完成</summary>
        event Action OnCompleted;

        // ------------------------------------------------------------------
        // 状态
        // ------------------------------------------------------------------

        HotUpdateState State { get; }

        /// <summary>是否有内容需要下载</summary>
        bool NeedUpdate { get; }

        int  TotalDownloadCount { get; }
        long TotalDownloadBytes { get; }

        // ------------------------------------------------------------------
        // 操作
        // ------------------------------------------------------------------

        /// <summary>执行完整热更流程（幂等，可重试）</summary>
        UniTask RunAsync(CancellationToken ct = default);

        /// <summary>回滚到指定版本</summary>
        UniTask RollbackAsync(string targetVersion, CancellationToken ct = default);

        /// <summary>清理本地过期缓存</summary>
        UniTask ClearUnusedCacheAsync();
    }

    public enum HotUpdateState
    {
        Idle,
        Initializing,
        GettingVersion,
        UpdatingManifest,
        CreatingDownloader,
        Downloading,
        Done,
        Error
    }
}
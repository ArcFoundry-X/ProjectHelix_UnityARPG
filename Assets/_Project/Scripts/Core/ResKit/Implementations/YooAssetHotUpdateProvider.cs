using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace YFramework.ResKit
{
    /// <summary>IHotUpdateProvider 的 YooAsset 实现</summary>
    public class YooAssetHotUpdateProvider : IHotUpdateProvider
    {
        // ------------------------------------------------------------------
        // 事件
        // ------------------------------------------------------------------
        public event Action<float, long, long> OnProgress;
        public event Action<string, string>    OnFileError;
        public event Action<string>            OnError;
        public event Action                    OnCompleted;

        // ------------------------------------------------------------------
        // 属性
        // ------------------------------------------------------------------
        public HotUpdateState State              { get; private set; } = HotUpdateState.Idle;
        public bool           NeedUpdate         { get; private set; }
        public int            TotalDownloadCount { get; private set; }
        public long           TotalDownloadBytes { get; private set; }

        // ------------------------------------------------------------------
        // 内部字段
        // ------------------------------------------------------------------
        private readonly ResourcePackage _package;
        private readonly int _maxConcurrent;
        private readonly int _retryCount;
        private string _latestVersion;

        public YooAssetHotUpdateProvider(
            ResourcePackage package,
            int maxConcurrent = 10,
            int retryCount    = 3)
        {
            _package       = package;
            _maxConcurrent = maxConcurrent;
            _retryCount    = retryCount;
        }

        // ------------------------------------------------------------------
        // IHotUpdateProvider 实现
        // ------------------------------------------------------------------

        public async UniTask RunAsync(CancellationToken ct = default)
        {
            try
            {
                await StepGetVersion(ct);
                await StepUpdateManifest(ct);
                await StepDownload(ct);

                SetState(HotUpdateState.Done);
                OnCompleted?.Invoke();
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                SetState(HotUpdateState.Error);
                OnError?.Invoke(e.Message);
                throw;
            }
        }

        public async UniTask RollbackAsync(string targetVersion, CancellationToken ct = default)
        {
            var op = _package.LoadPackageManifestAsync(new LoadPackageManifestOptions(targetVersion, 60));
            await op.ToUniTask(cancellationToken: ct);
            if (op.Status != EOperationStatus.Succeeded)
                throw new Exception($"[YooAssetHotUpdate] 回滚失败: {op.Error}");
        }

        public async UniTask ClearUnusedCacheAsync()
        {
            var op = _package.ClearCacheAsync(new ClearCacheOptions("ClearUnusedBundleFiles"));
            await op.ToUniTask();
        }

        // ------------------------------------------------------------------
        // 步骤
        // ------------------------------------------------------------------

        private async UniTask StepGetVersion(CancellationToken ct)
        {
            SetState(HotUpdateState.GettingVersion);
            var op = _package.RequestPackageVersionAsync();
            await op.ToUniTask(cancellationToken: ct);
            if (op.Status != EOperationStatus.Succeeded)
                throw new Exception($"获取版本失败: {op.Error}");
            _latestVersion = op.PackageVersion;
        }

        private async UniTask StepUpdateManifest(CancellationToken ct)
        {
            SetState(HotUpdateState.UpdatingManifest);
            var op = _package.LoadPackageManifestAsync(new LoadPackageManifestOptions(_latestVersion, 30));
            await op.ToUniTask(cancellationToken: ct);
            if (op.Status != EOperationStatus.Succeeded)
                throw new Exception($"更新清单失败: {op.Error}");
        }

        private async UniTask StepDownload(CancellationToken ct)
        {
            SetState(HotUpdateState.CreatingDownloader);
            var downloader = _package.CreateResourceDownloader(new ResourceDownloaderOptions(_maxConcurrent, _retryCount));

            TotalDownloadCount = downloader.TotalDownloadCount;
            TotalDownloadBytes = downloader.TotalDownloadBytes;

            if (TotalDownloadCount == 0) { NeedUpdate = false; return; }

            NeedUpdate = true;
            SetState(HotUpdateState.Downloading);

            downloader.DownloadProgressChanged += args =>
                OnProgress?.Invoke(args.TotalDownloadCount > 0 ? (float)args.CurrentDownloadCount / args.TotalDownloadCount : 0f,
                    args.CurrentDownloadBytes, args.TotalDownloadBytes);

            downloader.DownloadError += args =>
                OnFileError?.Invoke(args.FileName, args.ErrorInfo);

            downloader.StartDownload();
            await downloader.ToUniTask(cancellationToken: ct);

            if (downloader.Status != EOperationStatus.Succeeded)
                throw new Exception($"下载未完成: {downloader.Status}");
        }

        private void SetState(HotUpdateState s)
        {
            State = s;
            Debug.Log($"[YooAssetHotUpdate:{_package.PackageName}] → {s}");
        }
    }
}
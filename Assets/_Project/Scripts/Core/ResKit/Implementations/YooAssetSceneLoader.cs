using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;
using SceneHandle = YooAsset.SceneHandle;

namespace YFramework.ResKit
{
    public class YooAssetSceneLoader : ISceneLoader
    {
        private readonly ResourcePackage _package;

        public YooAssetSceneLoader(ResourcePackage package) 
            => _package = package;
        
        public async UniTask<ISceneHandle> LoadSceneAsync(string address, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true,
            IProgress<float> progress = null, CancellationToken ct = default)
        {
            var handle = _package.LoadSceneAsync(address, mode, allowSceneActivation: activateOnLoad);
            await handle.ToUniTask(progress, cancellationToken: ct);
            
            if (handle.Status != EOperationStatus.Succeeded)
                throw new Exception($"[YooAssetSceneLoader] load scene failed: {address}");

            return new YooAssetSceneHandle(handle, address);
        }

        public async UniTask UnloadSceneAsync(ISceneHandle handle)
        {
            if (handle is not YooAssetSceneHandle yooHandle || !yooHandle.IsValid) return;
            var op = yooHandle.Raw.UnloadSceneAsync();
            await op.ToUniTask();
        }
    }
    
    
    /// <summary>ISceneHandle 的 YooAsset 包装</summary>
    public class YooAssetSceneHandle : ISceneHandle
    {
        public SceneHandle Raw { get; }
        public bool IsValid  => Raw != null && Raw.IsValid;
        public string Address { get; }

        public YooAssetSceneHandle(SceneHandle raw, string address)
        {
            Raw     = raw;
            Address = address;
        }
    }
}
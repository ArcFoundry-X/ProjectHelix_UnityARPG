using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace YFramework.ResKit
{
    public class YooAssetLoader : IResourceLoader
    {
        private readonly ResourcePackage _package;

        //引用计数： address -> (handle, count)
        private readonly Dictionary<string, (AssetHandle Handle, int Count)> _refs = new();

        public int TrackedAssetCount => _refs.Count;

        public YooAssetLoader(ResourcePackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }
        
        public async UniTask InitializeAsync(CancellationToken ct = default)
        {
            // Package 已在 YooAssetFrameworkFactory 中初始化，此处仅作校验
            if (_package.InitializeStatus != EOperationStatus.Succeeded)
                throw new Exception($"[YooAssetLoader] Package {_package.PackageName} 未就绪");
            
            var manifestOp = _package.RequestPackageVersionAsync();
            await manifestOp.ToUniTask(cancellationToken: ct);
            
            var updateManifestOp = _package.LoadPackageManifestAsync(new LoadPackageManifestOptions(
                manifestOp.PackageVersion, 60));
            await updateManifestOp.ToUniTask(cancellationToken: ct);

            if (updateManifestOp.Status != EOperationStatus.Succeeded)
            {
                throw new Exception($"[YooAssetLoader] Package {_package.PackageName} manifest failed");
            }
            else
            {
                Debug.Log("[YooAsset loader] initialize success");
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string address, IProgress<float> process = null, CancellationToken ct = default) where T : Object
        {
            var handle = _package.LoadAssetAsync<T>(address);
            Retain(address, handle);

            await handle.ToUniTask(process, cancellationToken: ct);

            if (handle.Status != EOperationStatus.Succeeded)
            {
                Release(address);
                throw new Exception($"[YooAssetLoader] load asset failed: {address} | {handle.Error}");
            }

            return handle.AssetObject as T;
        }

        public T LoadAsset<T>(string address) where T : Object
        {
            var handle = _package.LoadAssetSync<T>(address);
            Retain(address, handle);

            if (handle.Status != EOperationStatus.Succeeded)
            {
                Release(address);
                throw new Exception($"[YooAssetLoader] 同步加载失败: {address}");
            }
            return handle.AssetObject as T;
        }

        public void ReleaseAsset(string address)
        {
            Release(address);
        }

        public void ReleaseAll()
        {
            foreach (var kv in _refs)
                kv.Value.Handle?.Release();
            _refs.Clear();
        }
        
        
        // ------------------------------------------------------------------
        // 引用计数内部方法
        // ------------------------------------------------------------------

        private void Retain(string address, AssetHandle handle)
        {
            if (_refs.TryGetValue(address, out var e))
                _refs[address] = (e.Handle, e.Count + 1);
            else
                _refs[address] = (handle, 1);
        }

        private void Release(string address)
        {
            if (!_refs.TryGetValue(address, out var e)) return;
            if (e.Count <= 1) { e.Handle?.Release(); _refs.Remove(address); }
            else _refs[address] = (e.Handle, e.Count - 1);
        }
    }
}
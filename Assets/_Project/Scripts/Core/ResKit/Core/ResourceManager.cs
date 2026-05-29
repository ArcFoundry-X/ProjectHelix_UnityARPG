using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace YFramework.ResKit
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        private IResourceLoader _resourceLoader;
        private ISceneLoader _sceneLoader;

        private bool _initialized;

        public void Setup(IResourceFrameworkFactory factory, object config = null)
        {
            _resourceLoader = factory.CreateResLoader(config);
            _sceneLoader = factory.CreateSceneLoader(_resourceLoader);
        }

        public async UniTask InitializeAsync(CancellationToken ct = default)
        {
            EnsureSetup();
            await _resourceLoader.InitializeAsync(ct);

            _initialized = true;
            Debug.Log("[ResourceManager] initialized");
        }

        public async UniTask<T> LoadAssetAsync<T>(string address, string group = null, IProgress<float> progress = null,
            CancellationToken ct = default) where T : Object
        {
            EnsureInitialized();
            return await _resourceLoader.LoadAssetAsync<T>(address, progress, ct);
        }

        public T LoadAsset<T>(string address) where T : Object
        {
            EnsureInitialized();
            return _resourceLoader.LoadAsset<T>(address);
        }

        public void ReleaseAsset(string address)
            => _resourceLoader.ReleaseAsset(address);

        public void ReleaseAll()
        {
            _resourceLoader.ReleaseAll();
        }

        public async UniTask<ISceneHandle> LoadSceneAsync(string address, LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true, IProgress<float> progress = null, CancellationToken ct = default)
        {
            EnsureInitialized();
            return await _sceneLoader.LoadSceneAsync(address, mode, activateOnLoad, progress, ct);
        }

        public async UniTask UnloadSceneAsync(ISceneHandle handle)
            => await _sceneLoader.UnloadSceneAsync(handle);

        private void EnsureSetup()
        {
            if (_resourceLoader == null)
                throw new InvalidOperationException(
                    "[ResourceManager] 未调用 Setup()，请先注入实现。");
        }

        private void EnsureInitialized()
        {
            EnsureSetup();
            if (!_initialized)
                throw new InvalidOperationException(
                    "[ResourceManager] 未调用 InitializeAsync()。");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            ReleaseAll();
        }
    }
}
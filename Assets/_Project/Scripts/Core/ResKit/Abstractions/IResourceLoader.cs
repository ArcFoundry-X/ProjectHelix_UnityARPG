using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace YFramework.ResKit
{
    /// <summary>
    /// 资源加载抽象接口
    /// </summary>
    public interface IResourceLoader
    {
        UniTask InitializeAsync(CancellationToken ct = default);

        UniTask<T> LoadAssetAsync<T>(
            string address, 
            IProgress<float> process = null, 
            CancellationToken ct = default) where T : UnityEngine.Object;

        /// <summary>
        /// 同步加载资源，仅限以缓存的资源，谨慎使用
        /// </summary>
        /// <param name="address"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T LoadAsset<T>(string address) where T : UnityEngine.Object;

        void ReleaseAsset(string address);

        void ReleaseAll();
        
        /// <summary>当前已追踪的资源数量</summary>
        int TrackedAssetCount { get; }
    }
}
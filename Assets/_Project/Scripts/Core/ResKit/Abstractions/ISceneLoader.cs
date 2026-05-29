using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace YFramework.ResKit
{
    /// <summary>
    /// 场景加载抽象接口
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>异步加载场景，返回场景句柄（用于后续卸载）</summary>
        UniTask<ISceneHandle> LoadSceneAsync(
            string address,
            LoadSceneMode mode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            IProgress<float> progress = null,
            CancellationToken ct = default);

        /// <summary>卸载场景</summary>
        UniTask UnloadSceneAsync(ISceneHandle handle);
    }

    /// <summary>
    /// 场景句柄抽象（屏蔽底层 SceneOperationHandle / AsyncOperationHandle 差异）
    /// </summary>
    public interface ISceneHandle
    {
        bool IsValid { get; }
        string Address { get; }
    }
}
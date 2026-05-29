using UnityEngine;
using YFramework.PoolKit;

namespace YFramework.ResKit
{
    
    /// <summary>
    /// 框架工厂接口：负责创建当前后端对应的实现对象
    ///
    /// 替换底层库时，只需换一个 IResourceFrameworkFactory 实现，
    /// ResourceManager 的代码完全不动。
    ///
    /// 内置工厂：
    ///   YooAssetFrameworkFactory  → 使用 YooAsset
    ///   AddressablesFrameworkFactory → 使用 Addressables（需另行实现）
    ///   ResourcesFrameworkFactory → 使用 UnityEngine.Resources（无热更）
    /// </summary>
    public interface IResourceFrameworkFactory
    {
        IResourceLoader      CreateResLoader(object config);
        ISceneLoader         CreateSceneLoader(IResourceLoader loader);
        IHotUpdateProvider   CreateHotUpdateProvider(object config);
    }
}
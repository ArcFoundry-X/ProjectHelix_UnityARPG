using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YFramework.PoolKit;
using YooAsset;

namespace YFramework.ResKit
{
    /// <summary>
    /// Remote resource URL query service.
    /// </summary>
    public class RemoteService : IRemoteService
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteService(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            List<string> result = new List<string>();
            result.Add($"{_defaultHostServer}/{fileName}");
            result.Add($"{_fallbackHostServer}/{fileName}");
            return result;
        }
    }
    
    public class YooAssetFrameworkFactory : IResourceFrameworkFactory
    {
        private readonly Dictionary<string, ResourcePackage> _packages = new();

        public async UniTask InitializeAllPackagesAsync(YooAssetConfig config, CancellationToken ct = default)
        {
            YooAssets.Initialize();
            
            foreach (var pkgCfg in config.Packages)
            {
                var pkg = YooAssets.CreatePackage(pkgCfg.PackageName);
                var initParams = BuildInitParams(pkgCfg, config.PlayMode);
            
                var op = pkg.InitializePackageAsync(initParams);
                await op.ToUniTask(cancellationToken: ct);
            
                if (op.Status != EOperationStatus.Succeeded)
                    throw new Exception($"[YooAsset] Package {pkgCfg.PackageName} 初始化失败: {op.Error}");
            
                _packages[pkgCfg.PackageName] = pkg;
                Debug.Log($"[YooAsset] Package {pkgCfg.PackageName} 就绪");
            }
        }

        public IResourceLoader CreateResLoader(object config)
        {
            var cfg = (YooAssetConfig)config;
            var defPkg = GetPackage(cfg.DefaultPackageName);
            return new YooAssetLoader(defPkg);
        }

        public ISceneLoader CreateSceneLoader(IResourceLoader loader)
        {
            var defPkg = GetFirstPackage();
            return new YooAssetSceneLoader(defPkg);
        }

        public IHotUpdateProvider CreateHotUpdateProvider(object config)
        {
            var cfg = (YooAssetPackageConfig)config;
            var pkg = GetPackage(cfg.PackageName);
            return new YooAssetHotUpdateProvider(pkg, cfg.MaxConcurrent, cfg.RetryCount);
        }

        public IHotUpdateProvider CrateHotUpdateProviderForPackage(
            string packageName, int maxConcurrent = 10, int retryCount = 3)
            => new YooAssetHotUpdateProvider(GetPackage(packageName), maxConcurrent, retryCount);

        private ResourcePackage GetPackage(string name)
        {
            if (_packages.TryGetValue(name, out var p)) return p;
            throw new Exception($"[YooAssetFactory] Package 未初始化: {name}");
        }

        private ResourcePackage GetFirstPackage()
        {
            foreach (var kv in _packages) return kv.Value;
            throw new Exception("[YooAssetFactory] 没有已初始化的 Package");
        }

        private static InitializePackageOptions BuildInitParams(YooAssetPackageConfig cfg, EPlayMode mode)
        {
            return mode switch
            {
                EPlayMode.EditorSimulateMode => BuildEditor(cfg),
                EPlayMode.OfflinePlayMode => new OfflinePlayModeOptions()
                {
                    BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters()
                },
                EPlayMode.HostPlayMode => new HostPlayModeOptions()
                {
                    //BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(),
                    CacheFileSystemParameters = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(
                        new DefaultRemoteServices(cfg.MainCDN, cfg.FallbackCDN))
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static InitializePackageOptions BuildEditor(YooAssetPackageConfig cfg)
        {
#if UNITY_EDITOR
            var buildResult = EditorSimulateBuildInvoker.Build(cfg.PackageName, (int)EBundleType.VirtualBundle);
            Debug.Log(buildResult.PackageRootDirectory);
            return new EditorSimulateModeOptions
            {
                EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(
                    buildResult.PackageRootDirectory)
            };
#else
            throw new Exception("EditorSimulateMode 只能在 Editor 下使用");
#endif
        }
    }
    public class DefaultRemoteServices : IRemoteService
    {
        private readonly string _main, _fallback;
        public DefaultRemoteServices(string main, string fallback) { _main = main; _fallback = fallback; }
        public IReadOnlyList<string> GetRemoteUrls(string fileName)
            => new ReadOnlyCollection<string>(new List<string> { $"{_main}/{fileName}", $"{_fallback}/{fileName}" });
    }
    
}
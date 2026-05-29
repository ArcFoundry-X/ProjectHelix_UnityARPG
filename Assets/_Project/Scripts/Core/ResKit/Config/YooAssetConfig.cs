using System.Collections.Generic;
using YooAsset;

namespace YFramework.ResKit
{
    public class YooAssetConfig
    {
        public EPlayMode                  PlayMode           = EPlayMode.HostPlayMode;
        public string                     DefaultPackageName = "CorePackage";
        public List<YooAssetPackageConfig> Packages          = new();
    }
    
    /// <summary>单个 Package 的配置</summary>
    public class YooAssetPackageConfig
    {
        public string PackageName;
        public string MainCDN;
        public string FallbackCDN;
        public int    MaxConcurrent = 10;
        public int    RetryCount    = 3;
    }
}
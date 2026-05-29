using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YFramework.HFSM;
using YFramework.ResKit;
using YooAsset;

public class ResPatchManager : MonoBehaviour
{
    public EPlayMode PlayMode;
    
    private async void Start()
    {
        YooAssetConfig yooAssetConfig = new YooAssetConfig()
        {
            PlayMode = PlayMode,
            DefaultPackageName = "DefaultPackage",
            Packages = new()
            {
                new YooAssetPackageConfig()
                {
                    PackageName = "DefaultPackage",
                    MainCDN = "http://127.0.0.1:8080/FileServer/DefaultPackage",
                    FallbackCDN = "http://127.0.0.1:8080/FileServer/DefaultPackage"
                },
                new YooAssetPackageConfig()
                {
                    PackageName = "DLCPackage",
                    MainCDN = "http://127.0.0.1:8080/FileServer/DLCPackage",
                    FallbackCDN = "http://127.0.0.1:8080/FileServer/DLCPackage"
                }
            }
        };
        
        var factory = new YooAssetFrameworkFactory();
        await factory.InitializeAllPackagesAsync(yooAssetConfig);

        ResourceManager.Instance.Setup(factory, yooAssetConfig);
        
        await ResourceManager.Instance.InitializeAsync();

        //热更新
        if (PlayMode == EPlayMode.HostPlayMode)
        {
            var runner = new HotUpdateRunner();
            foreach (var package in yooAssetConfig.Packages)
            {
                var provider = factory.CrateHotUpdateProviderForPackage(package.PackageName);
                runner.AddProvider(package.PackageName, provider);
            }
        }
        
        var asset = await ResourceManager.Instance.LoadAssetAsync<GameObject>("BlueIsland_Man");

        Instantiate(asset);
        
        ResourceManager.Instance.ReleaseAsset("BlueIsland_Man");
    }
  
}
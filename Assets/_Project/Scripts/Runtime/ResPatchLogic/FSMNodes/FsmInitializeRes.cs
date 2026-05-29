using System.Collections;
using UnityEngine;
using YFramework.HFSM;
using YooAsset;

public class FsmInitializeRes : StateBase
{
    public override void OnEnter()
    {
        base.OnEnter();

        GameMain.Instance.StartCoroutine(InitPackage());
    }
    
    private IEnumerator InitPackage()
    {
        var playMode = (EPlayMode)OwnerMachine.TryGetBlackboardValue("PlayMode");
        var packageName = "DefaultPackage";

        Debug.Log($"package name: {packageName}");
        // Create package.
        if (!YooAssets.TryGetPackage(packageName, out var package))
            package = YooAssets.CreatePackage(packageName);

        // Editor simulation mode.
        InitializePackageOperation initOp = null;
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualBundle);
            var packageRoot = buildResult.PackageRootDirectory;
            var createParameters = new EditorSimulateModeOptions();
            createParameters.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualWebglMode, true);
            createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadMode, true);
            createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadSpeed,
                1024 * 1000);
            createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMinFrame, 5);
            createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMaxFrame, 10);
            initOp = package.InitializePackageAsync(createParameters);
        }

        // Offline play mode.
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeOptions();
            createParameters.BuiltinFileSystemParameters =
                FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
            initOp = package.InitializePackageAsync(createParameters);
        }

        yield return initOp;

        if (initOp.Status != EOperationStatus.Succeeded)
        {
            Debug.LogError($"初始化失败: {initOp.Error}");
            yield break;
        }
        Debug.Log("初始化成功");

        var manifestOp = package.RequestPackageVersionAsync();
        yield return manifestOp;
        
        var updateMainfestOp = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(
            manifestOp.PackageVersion, 60));

        yield return updateMainfestOp;

        if (updateMainfestOp.Status != EOperationStatus.Succeeded)
        {
            Debug.LogError($"load manifest failed {updateMainfestOp.Error}");
            yield break;
        }
        
        LoadPackage(package);
    }

    private async void LoadPackage(ResourcePackage package)
    {
        AssetHandle handle = package.LoadAssetAsync<GameObject>("BlueIsland_Man");
        await handle;

        await handle.InstantiateAsync();
        Debug.Log("instantiate");
        
        handle.Release();
        
    }
}

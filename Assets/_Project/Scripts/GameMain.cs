using System;
using System.Collections;
using UnityEngine;
using YooAsset;

public class GameMain : MonoSingleton<GameMain>
{
    /// <summary>
    /// Resource system play mode.
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    protected override void Awake()
    {
        Application.targetFrameRate = 30;
        Application.runInBackground = true;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
       
    }

}
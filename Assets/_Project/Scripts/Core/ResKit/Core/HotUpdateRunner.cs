using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YFramework.ResKit
{
    /// <summary>
    /// 热更新执行器
    /// </summary>
    public class HotUpdateRunner
    {
        /// <summary>单个分组进度：(分组名, 归一化进度, 已下载字节, 总字节)</summary>
        public event Action<string, float, long, long> OnGroupProgress;

        /// <summary>单个分组错误：(分组名, 错误信息)</summary>
        public event Action<string, string> OnGroupError;

        /// <summary>整体进度：(归一化进度)</summary>
        public event Action<float> OnTotalProgress;

        /// <summary>所有分组完成</summary>
        public event Action OnAllCompleted;
        
        private readonly List<(string Name, IHotUpdateProvider Provider)> _providers = new();

        public void AddProvider(string groupName, IHotUpdateProvider provider)
            => _providers.Add((groupName, provider));

        public async UniTask RunAllAsync(CancellationToken ct = default)
        {
            for (int i = 0; i < _providers.Count; i++)
            {
                var (name, provider) = _providers[i];
                ct.ThrowIfCancellationRequested();

                provider.OnProgress += (p, done, total) =>
                {
                    OnGroupProgress?.Invoke(name, p, done, total);
                    // 整体进度：当前分组占比 + 已完成分组占比
                    float totalP = (i + p) / _providers.Count;
                    OnTotalProgress?.Invoke(totalP);
                };
                
                provider.OnFileError += (file, err) =>
                    OnGroupError?.Invoke(name, $"{file}: {err}");

                await provider.RunAsync(ct);
            }
            
            OnAllCompleted?.Invoke();
            Debug.Log("[HotUpdateRunner] 全部分组热更完成");
        }
    }
}
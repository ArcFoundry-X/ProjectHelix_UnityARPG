using UnityEngine;

namespace YFramework.PoolKit
{
    public interface IPoolItem
    {
        void OnAllocate();
    
        void OnRecycle();
    }
}


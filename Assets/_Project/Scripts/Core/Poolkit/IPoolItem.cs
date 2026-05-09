using UnityEngine;

public interface IPoolItem
{
    void OnAllocate();
    
    void OnRecycle();
}

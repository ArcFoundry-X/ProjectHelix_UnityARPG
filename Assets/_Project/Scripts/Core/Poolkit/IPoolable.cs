using UnityEngine;

public interface IPoolable
{
    /// <summary>
    /// 从池中取出时调用
    /// </summary>
    void OnAllocate();

    /// <summary>
    /// 回收时调用
    /// </summary>
    void OnRecycle();
}

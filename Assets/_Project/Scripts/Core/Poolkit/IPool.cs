using System;

namespace YFramework.PoolKit
{
    /// <summary>
    /// 非泛型池接口，用于统一持有和管理所有类型的池。
    /// Manager 通过此接口操作池，无需强转。
    /// </summary>
    public interface IPool
    {
        /// <summary>已创建的对象总数（含池中和活跃中）</summary>
        int CountAll { get; }
 
        /// <summary>当前在外部使用中的对象数</summary>
        int CountActive { get; }
 
        /// <summary>当前缓存在池中的对象数</summary>
        int CountInPool { get; }
 
        /// <summary>池持有的对象类型</summary>
        Type ObjectType { get; }
 
        /// <summary>清空池，销毁所有缓存对象</summary>
        void Clear();
    }
 
    /// <summary>
    /// 泛型池接口，提供类型安全的 Allocate / Recycle。
    /// ObjectPool&lt;T&gt; 实现此接口；Manager 内部通过此接口调用。
    /// </summary>
    public interface IPool<T> : IPool
    {
        T Allocate();
        void Recycle(T item);
    }
}
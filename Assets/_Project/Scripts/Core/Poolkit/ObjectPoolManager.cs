using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
{
    private Dictionary<Type, object> _poolMap = new();

    public void Register<T>(Func<T> createFunc, Action<T> actionOnAllocate = null,Action<T> actionOnRecycle = null, int initSize = 0, int capacity = 64)
        where T : Component, IPoolItem
    {
        if (_poolMap.ContainsKey(typeof(T)))
        {
            return;
        }

        var pool = new ObjectPool<T>(createFunc, actionOnAllocate, actionOnRecycle, initSize, capacity);
        _poolMap.Add(typeof(T), pool);
    }

    public T Allocate<T>() where T : Component, IPoolItem
    {
        return GetPool<T>()?.Allocate();
    }

    public void Recycle<T>(T item) where T : Component, IPoolItem
    {
        GetPool<T>()?.Recycle(item);
    }

    public void DestroyPool<T>() where T : Component, IPoolItem
    {
        GetPool<T>()?.Clear();
        _poolMap.Remove(typeof(T));
    }

    public (int total, int active, int cached) GetStats<T>()
        where T : Component, IPoolItem
    {
        var p = GetPool<T>();
        return (p.CountAll, p.CountActive, p.CountInPool);
    }

    private ObjectPool<T> GetPool<T>()
        where T : Component, IPoolItem
    {
        if (_poolMap.TryGetValue(typeof(T), out var pool))
            return (ObjectPool<T>)pool;

        throw new InvalidOperationException(
            $"[ObjectPoolManager] Pool '{typeof(T)}' not registered. Call Register() first.");
    }
}
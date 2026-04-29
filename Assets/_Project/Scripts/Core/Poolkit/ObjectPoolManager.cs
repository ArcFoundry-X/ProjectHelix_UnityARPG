using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private Dictionary<Type, IObjectPool> _poolsMap = new();

    public T Allocate<T>() where T : MonoBehaviour, IPoolable
    {
        if (_poolsMap.TryGetValue(typeof(T), out var pool))
        {
            return (pool as ObjectPool<T>).Allocate();
        }

        return new T();
    }
}
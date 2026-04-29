using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : IObjectPool where T : MonoBehaviour, IPoolable
{
    private Stack<T> _stack;

    public T Allocate()
    {
        if (_stack.Count != 0)
            return _stack.Pop();

        return new T();
    }


    public void Recycle(T obj)
    {
        _stack.Push(obj);
    }

    public void ReleaseAll()
    {
        
    }

    public void Clear()
    {
        
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YFramework.PoolKit
{
    public class ObjectPool<T> : IPool<T> where T : Component, IPoolItem
    {
        private readonly int _capacity;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnAllocate;
        private readonly Action<T> _actionOnRecycle;

        private readonly Stack<T> _stack;
        private readonly HashSet<T> _cachedSet;

        public int CountInPool => _stack.Count;

        public int CountAll { get; private set; }

        public int CountActive => CountAll - CountInPool;

        public Type ObjectType => typeof(T);


        public ObjectPool(Func<T> createFunc, Action<T> actionOnAllocate = null, Action<T> actionOnRecycle = null,
            int initSize = 0, int capacity = 64)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnAllocate = actionOnAllocate;
            _actionOnRecycle = actionOnRecycle;
            _capacity = capacity;

            _stack = new Stack<T>(Mathf.Max(initSize, 8));
            _cachedSet = new HashSet<T>();

            // 真正预热：提前创建并缓存对象
            for (int i = 0; i < initSize; i++)
            {
                T obj = _createFunc();
                CountAll++;
                obj.OnRecycle();
                _actionOnRecycle?.Invoke(obj);
                _stack.Push(obj);
                _cachedSet.Add(obj);
            }
        }

        public T Allocate()
        {
            T obj;
            if (_stack.Count == 0)
            {
                obj = _createFunc();
                CountAll++;
            }
            else
            {
                obj = _stack.Pop();
                _cachedSet.Remove(obj);
            }

            obj?.OnAllocate();

            _actionOnAllocate?.Invoke(obj);

            return obj;
        }

        public void Recycle(T item)
        {
            if (item == null || !item) return;

            if (_cachedSet.Contains(item))
            {
                Debug.LogWarning($"[ObjectPool] Attempted to recycle an item that is already in pool: {item.name}",
                    item);
                return;
            }

            item.OnRecycle();
            _actionOnRecycle?.Invoke(item);

            if (_stack.Count < _capacity)
            {
                _stack.Push(item);
                _cachedSet.Add(item);
            }
            else
            {
                CountAll--;
                Object.Destroy(item.gameObject);
            }
        }

        public void Clear()
        {
            int destroyedCount = _stack.Count;
            while (_stack.Count > 0)
            {
                Object.Destroy(_stack.Pop().gameObject);
            }

            _cachedSet.Clear();
            CountAll -= destroyedCount;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace YFramework.HFSM
{
    /// <summary>
    /// 黑板：状态机内部各状态之间共享数据的容器
    /// 使用字典存储任意类型的值，通过 key 读写
    /// </summary>
    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = new();
        
        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }
 
        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }
 
        public bool TryGet<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }
 
        public bool Has(string key) => _data.ContainsKey(key);
 
        public void Remove(string key) => _data.Remove(key);
 
        public void Clear() => _data.Clear();
        
        public void DebugPrint()
        {
            foreach (var kv in _data)
                Debug.Log($"[Blackboard] {kv.Key} = {kv.Value}");
        }
    }
}

using UnityEngine;

/// <summary>
/// 通用 MonoBehaviour 单例基类。
/// </summary>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    // 泛型静态字段：每个 T 独立一份，不会互相覆盖
    private static T _instance;
    
    // 用于防止退出时创建幽灵对象
    private static bool _isQuitting = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
            {
                Debug.LogWarning($"[{typeof(T).Name}] 应用正在退出，返回 null 而非创建新实例");
                return null;
            }

            if (_instance == null)
            {
                // 先尝试在场景中找（处理手动放置在场景里的情况）
                _instance = FindFirstObjectByType<T>();

                // 场景里没有，自动创建
                if (_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    // AddComponent 会触发 Awake，Awake 里会赋值 _instance
                    go.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // 场景中存在重复实例（如场景切换时旧实例未销毁），销毁新的
            Debug.LogWarning($"[{typeof(T).Name}] 检测到重复实例，销毁多余的 GameObject: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
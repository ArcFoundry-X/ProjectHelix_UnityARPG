using UnityEngine;

public interface IObjectPool
{
    void ReleaseAll();

    void Clear();
}

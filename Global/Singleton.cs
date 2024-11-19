using UnityEngine;

namespace Helpers
{
public class Singleton<T> : MonoBehaviour, ISingleton where T : Component
{
    public static T Instance { get; protected set; }
    public static bool HasInstance => Instance != null;
    public bool IsSet => HasInstance;

    protected virtual void Awake()
    {
        if (!HasInstance)
        {
            Instance = this as T;
            transform.parent = null;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (Instance != this)
                Destroy(gameObject);
        }
    }
}
}
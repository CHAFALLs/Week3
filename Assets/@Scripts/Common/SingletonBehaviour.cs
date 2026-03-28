using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    protected bool m_IsDestroyOnLoad = false;
    protected static T m_Instance;
    public static T Instance => m_Instance;

    private void Awake()
    {
        Initialize(); 
    }

    protected virtual void Initialize()
    {
        if (m_Instance == null)
        {
            m_Instance = (T)this;
            // 루트 오브젝트일 때만 DontDestroyOnLoad
            if (!m_IsDestroyOnLoad && transform.parent == null)
                DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        Dispose();
    }

    protected virtual void Dispose()
    {
        m_Instance = null;
    }
}

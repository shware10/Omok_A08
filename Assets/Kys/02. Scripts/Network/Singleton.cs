using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                var t = FindFirstObjectByType<T>();

                if (t != null)
                    instance = t;
                else
                {
                    var newObj = new GameObject(typeof(T).ToString());
                    newObj.AddComponent<T>();

                    instance = newObj.GetComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            Debug.Log("인스턴스 없어서 할당");
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("인스턴스 있어서 제거");
            Destroy(gameObject);
        }
        
            
    }
}

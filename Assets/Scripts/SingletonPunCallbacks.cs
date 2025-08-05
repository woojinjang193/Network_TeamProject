using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SingletonPunCallbacks<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance != null) return instance;

                GameObject go = new GameObject($"{typeof(T)}");
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);

            }
            return instance;
        }
    }
    protected virtual void Awake()
    {
        Init();
    }
    protected virtual void Init()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    protected void OnApplicationQuit()
    {
        instance = null;
    }


    public virtual void DestroyManager()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
    }
}

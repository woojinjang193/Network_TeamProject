using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour  
{
    protected static T Instance;
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T; // this as T는 객체를 T 타입으로 바꾸려 시도해보고 안되면 null을 반환
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public virtual void Init() 
    {
    }

}

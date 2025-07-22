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
            Instance = this as T; // this as T�� ��ü�� T Ÿ������ �ٲٷ� �õ��غ��� �ȵǸ� null�� ��ȯ
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

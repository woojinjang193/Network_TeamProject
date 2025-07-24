using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;

public class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;
    public static FirebaseManager Instance { get { return instance; } }

    private static FirebaseApp app; 
    public static FirebaseApp App { get { return app; } }

    private static FirebaseAuth auth;   
    public static FirebaseAuth Auth { get { return auth; } }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("파이어 베이스 설정이 모두 충족되어 사용할 수 있는 상황");
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError("파이어 베이스 설정이 충족되지 않았습니다. 이유: {dependencyStatus}");
                app = null;
                auth = null;
            }
        });
    }
}

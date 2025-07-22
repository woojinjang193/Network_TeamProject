using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static GameManager GetInstance() => Instance;
    private void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("게임매니저 스타트");
    }

    public void GameStart()
    {
        Debug.Log("게임 스타트");
    }

    public void GameEnd()
    {
        Debug.Log("게임 엔드");
    }


   

}

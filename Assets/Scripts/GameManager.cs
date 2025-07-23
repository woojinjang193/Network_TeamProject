using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static GameManager GetInstance() => Instance;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("���ӸŴ��� ��ŸƮ");
    }

    public void GameStart()
    {
        Debug.Log("���� ��ŸƮ");
    }

    public void GameEnd()
    {
        Debug.Log("���� ����");
    }


   

}

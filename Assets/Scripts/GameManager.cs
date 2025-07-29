using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool IsGameEnd {  get; private set; }

    private Dictionary<Collider, PlayerTestController> playerDic = new();
    //플레이어의 콜라이더와 컨트롤러를 넣을 딕셔너리 

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Debug.Log("게임매니저 스타트");
    }

    public void RegisterPlayer(Collider col, PlayerTestController playerController)
    {
        playerDic[col] = playerController;
        //호출되면 그 플레이어의 콜라이더를 딕셔너리에 추가
    }

    public PlayerTestController GetPlayer(Collider col)
    {
        playerDic.TryGetValue(col, out PlayerTestController playerTestController);
        //키를 넣으면 playerTestController를 반환
        return playerTestController;

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

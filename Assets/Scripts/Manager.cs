using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    public static FirebaseManager FB;
    public static NetworkManager Net;
    public static UIManager UI;
    public static GameManager Game;
    public static AudioManager Audio;
    public static GridManager Grid;

    protected override void Awake()
    {
        base.Awake();
        Net = NetworkManager.Instance;
        FB = FirebaseManager.Instance;
        Audio = AudioManager.Instance;
        UI = UIManager.Instance;
        
        
        Game = FindObjectOfType<GameManager>();
        Grid = FindObjectOfType<GridManager>();


    }

    //public override void OnJoinedRoom()
    //{
    //    if (Grid == null && PhotonNetwork.IsMasterClient)
    //    // 프리팹에서 GridManager 생성.네트워크 환경에서 생성이니 마스터클라이언트가 처음에만 생성해주면 됨
    //    {
    //        GameObject gridManager = PhotonNetwork.Instantiate("GridManager", Vector3.zero, Quaternion.identity);
    //        Grid = gridManager.GetComponent<GridManager>();
    //    }
    //}

}

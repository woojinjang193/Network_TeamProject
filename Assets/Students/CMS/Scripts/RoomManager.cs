using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;


public class RoomManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // 방 시작 버튼 (현재 미사용)
    [SerializeField] private Button leaveButton; // 방 나가기 버튼

    [SerializeField] private GameObject playerPanelItemPrefabs;  // 플레이어 패널 프리팹
    [SerializeField] private Transform playerPanelContent;       // 플레이어 패널들이 추가될 부모 오브젝트

    private Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();

    private void Awake()
    {
        leaveButton.onClick.AddListener(()=> NetworkManager.Instance.LeaveRoom()); // 방 나가기 버튼 클릭 시 호출
    }

    public void PlayerPanelSpawn(Player player) // 새로운 플레이어가 들어왔을 때 패널 추가
    {
        GameObject obj = Instantiate(playerPanelItemPrefabs);      // 프리팹 생성
        obj.transform.SetParent(playerPanelContent);
        PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
        //초기화
        item.Init(player);
        playerPanels.Add(player.ActorNumber, item);
    }

    public void PlayerPanelspawn()  // 현재 방의 모든 플레이어 패널 일괄 생성
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerPanelItemPrefabs);
            obj.transform.SetParent(playerPanelContent);
            PlayerPanelItem item = obj.GetComponent<PlayerPanelItem>();
            //초기화
            item.Init(player);
            playerPanels.Add(player.ActorNumber, item);
        }
    }
}

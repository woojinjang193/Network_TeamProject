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

    public Dictionary<int, PlayerPanelItem> playerPanels = new Dictionary<int, PlayerPanelItem>();

    private void Awake()
    {
        leaveButton.onClick.AddListener(()=> NetworkManager.Instance.LeaveRoom()); // 방 나가기 버튼 클릭 시 호출
        startButton.onClick.AddListener(OnClickGameStart);
    }

    private void OnClickGameStart()
    {
        PhotonNetwork.LoadLevel("GameScene"); //시작시 GameScene 로드
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

    public void PlayerPanelRemove(Player player) // 플레이어가 나갔을 때 패널 제거
    {
        if (playerPanels.TryGetValue(player.ActorNumber, out PlayerPanelItem item))
        {
            Destroy(item.gameObject); // 패널 오브젝트 제거
            playerPanels.Remove(player.ActorNumber); // 딕셔너리에서 제거
        }
    }
    public void CheckAllReady()
    {
        // 모든 플레이어의 Ready 상태 확인
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Ready", out object value))
            {
                if (!(bool)value)
                {
                    SetStartButtonActive(false);
                    return; // 한 명이라도 false면 리턴
                }
            }
            else
            {
                SetStartButtonActive(false);
                return; // Ready 값이 없는 경우도 false 취급
            }
        }

        // 모든 플레이어가 Ready 상태면 Start 버튼 활성화 (호스트만)
        SetStartButtonActive(PhotonNetwork.IsMasterClient);
    }

    private void SetStartButtonActive(bool isActive)
    {
        startButton.gameObject.SetActive(isActive);
    }
}

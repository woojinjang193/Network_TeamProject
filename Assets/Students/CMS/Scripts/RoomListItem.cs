// using Photon.Pun;
// using Photon.Realtime;
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class RoomListItem : MonoBehaviourPun
// {
//     [SerializeField] private TMP_Text roomNameText;  // 방 이름 텍스트
//     [SerializeField] private TMP_Text maxPlayerText; // 현재 인원 / 최대 인원 텍스트
//     [SerializeField] private Button joinButton;
//     private string roomName;
//
//     public void Init(RoomInfo info)
//     {
//         roomName = info.Name;
//         roomNameText.text = roomName; //UI에 이름 표시
//         if(!info.CustomProperties.TryGetValue("tS", out object playerCount))
//         {
//             Debug.Log($"룸{info.Name} 커스텀 프로퍼티의 키 값을  가져오는데 실패함 tS");
//         }
//         maxPlayerText.text = $"{(int)playerCount} / {info.MaxPlayers}"; // 인원 표시
//         
//
//         joinButton = GetComponent<Button>();
//         joinButton.interactable = (int)playerCount < 8;
//         joinButton.onClick.AddListener(JoinRoom);
//     }
//
//     private void JoinRoom()
//     {
//         if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
//         {
//             PhotonNetwork.JoinRoom(roomName);
//             joinButton.onClick.RemoveAllListeners();
//         }
//         else
//         {
//             Debug.Log("아직 로비에 입장하지 않음.");
//         }
//     }
// }

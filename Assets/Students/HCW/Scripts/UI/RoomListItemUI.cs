using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private string roomName;

    public void Setup(Photon.Realtime.RoomInfo info)
    {
        roomName = info.Name;
        roomNameText.text = info.Name;

        if (!info.CustomProperties.TryGetValue("tS", out object playerCount))
        {
            Debug.Log($"룸{info.Name} 커스텀 프로퍼티의 키 값을 가져오는데 실패함 tS");
        }

        Debug.Log($"아이템UI 합:{playerCount}");
        playerCountText.text = $"{playerCount} / {info.MaxPlayers}";
        joinButton.interactable = (int)playerCount < 8;
        joinButton.onClick.AddListener(OnJoinButtonClick);
    }

    private void OnJoinButtonClick()
    {
        Debug.Log($"방 참가 시도: {roomName}");
        if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        {
            Manager.Net.JoinRoom(roomName);
            joinButton.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.Log("방 참가 실패. 아직 로비에 입장하지 않음.");
        }
    }
}

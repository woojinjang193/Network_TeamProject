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
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        joinButton.onClick.AddListener(OnJoinButtonClick);
    }

    private void OnJoinButtonClick()
    {
        // 포톤 방 입장 로직 구현
        if (Photon.Pun.PhotonNetwork.InLobby)
        {
            Photon.Pun.PhotonNetwork.JoinRoom(roomName);
        }
    }
}

using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomUIInitializer : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("[RoomUIInitializer] LoginScene 진입 → UIManager 재초기화 시도");

        UIManager.Instance.Reinitialize();

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[RoomUIInitializer] PhotonNetwork.InRoom = true → RoomUI 활성화");

            UIManager.Instance.ReplaceUI(typeof(RoomUI));

            var roomManager = FindObjectOfType<RoomManager>();
            if (roomManager != null)
            {
                roomManager.OnRoomJoined();
            }
            else
            {
                Debug.LogWarning("[RoomUIInitializer] RoomManager 없음!");
            }
        }
        else
        {
            Debug.Log("[RoomUIInitializer] 현재 방에 없음 → LobbyUI 표시");
            UIManager.Instance.ReplaceUI(typeof(LobbyUI));
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

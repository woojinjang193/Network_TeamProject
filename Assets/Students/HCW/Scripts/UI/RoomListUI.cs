using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListUI : BaseUI
{
    [Header("방 리스트")]
    [SerializeField] private Transform roomList;
    [SerializeField] private GameObject roomListPrefab; // 각 방 정보를 표시할 UI 프리팹
    [SerializeField] private Button backButton;

    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();

        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        // TODO: 방 목록을 Photon에서 받아와서 업데이트하는 로직 추가
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        if (uiManager != null)
        {
            uiManager.CloseCurrentUI(); // 이전 UI (LobbyUI)로 돌아감
        }
    }

}

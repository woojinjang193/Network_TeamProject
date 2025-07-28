using ExitGames.Client.Photon; 
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Image hostImage;
    [SerializeField] private Image readyImage;

    [SerializeField] private Button readyButton;

    private bool isReady;



    public void Init(Player player)
    {
        nameText.text = player.NickName;
        hostImage.enabled = player.IsMasterClient; // 호스트 여부 표시
        readyButton.interactable = player.IsLocal; // 로컬 플레이어만 준비 버튼 활성화

        if (!player.IsLocal)
            return;

        isReady = false;

        ReadyPropertiesUpdate();
        readyButton.onClick.AddListener(ReadyButtonClick);
    }

    public void ReadyButtonClick()
    {
        isReady = !isReady;

        readyText.text = isReady ? "Ready" : "Click Ready";
        readyImage.color = isReady ? Color.green : Color.white;

        ReadyPropertiesUpdate();
    }
    public void ReadyPropertiesUpdate()
    {
        ExitGames.Client.Photon.Hashtable playerProperty = new ExitGames.Client.Photon.Hashtable();
        playerProperty["Ready"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperty);
    }

    public void ReadyCheck(Player player)
    {
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("Ready", out object value))
        {
            isReady = (bool)value;  
        }

        readyText.text = isReady ? "Ready" : "Click Ready";
        readyImage.color = isReady ? Color.green : Color.white;
    }
}

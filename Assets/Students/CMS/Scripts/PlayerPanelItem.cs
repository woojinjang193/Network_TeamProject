using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPanelItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    public void Init(Player player)
    {
        nameText.text = player.NickName;
    }
}

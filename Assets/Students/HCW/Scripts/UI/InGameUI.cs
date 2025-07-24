using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : BaseUI
{
    [System.Serializable]
    public class PlayerStatusIcon
    {
        public Image weaponIcon; // 무기 아이콘
        public GameObject deadOverlay; // 사망 시 표시될 오버레이

        public void SetStatus(bool isAlive)
        {
            deadOverlay.SetActive(!isAlive);
        }

        public void SetWeapon(Sprite icon)
        {
            weaponIcon.sprite = icon;
        }
    }

    [Header("타이머")]
    [SerializeField] private TextMeshProUGUI tmp_Timer;

    [Header("잉크 게이지")]
    [SerializeField] private Slider slider_InkGauge;

    [Header("팀 상태")]
    [SerializeField] private List<PlayerStatusIcon> teamA_Icons; // A팀
    [SerializeField] private List<PlayerStatusIcon> teamB_Icons; // B팀


    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    // 남은 시간을 UI에 업데이트
    public void UpdateTimer(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        tmp_Timer.text = $"{minutes:D2}:{seconds:D2}";
    }

    // 잉크 용량 업데이트
    public void UpdateInkGauge(float currentInk, float maxInk)
    {
        slider_InkGauge.value = currentInk / maxInk;
    }

    // 잉크 게이지 활성화상태 설정 (상시 활성화 상태로 바꿀시 필요없어짐)
    public void SetInkGaugeActive(bool isActive)
    {
        slider_InkGauge.gameObject.SetActive(isActive);
    }

    // 인원들의 생존상태를 업데이트
    public void UpdatePlayerStatus(bool isTeamA, int playerIndex, bool isAlive)
    {
        List<PlayerStatusIcon> targetTeam = isTeamA ? teamA_Icons : teamB_Icons;
        if (playerIndex >= 0 && playerIndex < targetTeam.Count)
        {
            targetTeam[playerIndex].SetStatus(isAlive);
        }
    }

    // 플레이어 프로필을 무기 이미지로 설정
    public void SetPlayerWeapon(bool isTeamA, int playerIndex, Sprite weaponIcon)
    {
        List<PlayerStatusIcon> targetTeam = isTeamA ? teamA_Icons : teamB_Icons;
        if (playerIndex >= 0 && playerIndex < targetTeam.Count)
        {
            targetTeam[playerIndex].SetWeapon(weaponIcon);
        }
    }
}

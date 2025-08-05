using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : BaseUI
{
    private Canvas _canvas;
    private PlayerController myPlayer; // 내 플레이어 참조

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

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        // 내 플레이어를 아직 못 찾았다면 탐색
        if (myPlayer == null)
        {
            FindMyPlayer();
            if (myPlayer == null) return; // 아직도 못찾았으면 Update 종료
        }

        // 플레이어 상태에 따라 잉크 게이지 활성화 여부 결정
        if (myPlayer.stateMachine != null && myPlayer.highStateDic.ContainsKey(HighState.SquidForm))
        {
            bool isSquidForm = myPlayer.stateMachine.CurrentState == myPlayer.highStateDic[HighState.SquidForm];
            SetInkGaugeActive(isSquidForm);
        }

        // 플레이어를 찾았다면, 잉크 게이지 업데이트
        if (myPlayer.inkParticleGun != null)
        {
            UpdateInkGauge(myPlayer.inkParticleGun.currentInk, myPlayer.inkParticleGun.maxInk);
        }
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        StartCoroutine(FindAndSetPlayerCamera());
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    private void FindMyPlayer()
    {
        var players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            if (player.photonView.IsMine)
            {
                myPlayer = player;
                return; // 찾았으면 바로 종료
            }
        }
    }

    private IEnumerator FindAndSetPlayerCamera()
    {
        // 한 프레임 대기
        yield return null;

        if (myPlayer == null) FindMyPlayer();

        if (myPlayer != null && myPlayer.mainCamera != null)
        {
            if (_canvas != null)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = myPlayer.mainCamera;
                _canvas.planeDistance = 1; // 카메라와 UI 사이의 거리 (적절히 조절)
            }
        }
        else
        {
            Debug.LogWarning("InGameUI: 내 플레이어 컨트롤러 또는 메인 카메라를 찾을 수 없습니다.");
        }
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
        if(slider_InkGauge != null)
        {
            slider_InkGauge.value = currentInk / maxInk;
        }
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

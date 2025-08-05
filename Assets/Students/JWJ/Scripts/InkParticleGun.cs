using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class InkParticleGun : MonoBehaviourPun
{
    [Header("파티클 설정")]
    [SerializeField] private float particleSpeed = 20f;
    private TeamColorInfo teamColorInfo; //팀 컬러 정보
    [SerializeField] private ParticleSystem mainParticle; //잉크 줄기
    [SerializeField] private ParticleSystem fireEffect; // 무기 주변에 잉크가 튀는 연출
    //[SerializeField] private ParticleSystem floorInkEffect; // 충돌부분에 잉크 퍼지는 연출
    [SerializeField] private InkParticleCollision particleCollision; //잉크 충돌 스크립트

    [Header("잉크 탱크 설정")]
    public float maxInk = 100f;
    public float currentInk;
    public float inkConsumptionRate = 15f; // 초당 잉크 소모량
    public float inkRecoveryRate = 30f; // 초당 잉크 회복량

    private bool isFiring = false;

    // 게임시작시 파티클을 off 시켜놓기위해 EmissionModule을 담을 변수들
    private ParticleSystem.EmissionModule mainEmission;
    private ParticleSystem.EmissionModule fireEmission;

    //파티클 색 변경을 위해 MainModule 을 담을 변수들
    private ParticleSystem.MainModule mainParticleMain;
    private ParticleSystem.MainModule fireEffectMain;
    //private ParticleSystem.MainModule floorInkEffectMain;

    private Team currentTeam = Team.None;

    private ParticleSystem.MinMaxCurve startSpeedCurve;///
    private float lastParticleSpeed = 0;

    private void Awake()
    {
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        //팀 컬러정보 받아옴

        //emission 들
        mainEmission = mainParticle.emission;
        fireEmission = fireEffect.emission;

        //main 들
        mainParticleMain = mainParticle.main;
        fireEffectMain = fireEffect.main;
        //floorInkEffectMain = floorInkEffect.main;///

        mainEmission.enabled = false;
        //비활성화
        fireEmission.enabled = false;
        //비활성화 

        startSpeedCurve = new ParticleSystem.MinMaxCurve(particleSpeed, particleSpeed + 5);///
    }

    private void Start()
    {
        currentInk = maxInk; // 잉크 초기화
        
        SetTeamColor(currentTeam);

        mainParticleMain.startSpeed = startSpeedCurve; ///
    }

    private void Update()
    {
        if (isFiring && photonView.IsMine)
        {
            if (currentInk > 0)
            {
                currentInk -= inkConsumptionRate * Time.deltaTime;
            }
            else
            {
                currentInk = 0;
                isFiring = false;
                mainEmission.enabled = false;
                fireEmission.enabled = false;
            }
        }
    }

    [PunRPC]
    public void FireParticle(Team team, bool mouseButtonDown) //활성화 
    {
        this.isFiring = mouseButtonDown;

        if (currentInk <= 0)
        {
            this.isFiring = false;
        }

        UpdateStartSpeed();

        //mainParticleMain.startSpeed = particleSpeed;
        //파티클 on off 설정. isFiring 상태에 따라 결정
        mainEmission.enabled = this.isFiring;
        fireEmission.enabled = this.isFiring;

        if (team != currentTeam)
            //플레이어컨트롤러에서 넘겨받은 팀이 현재팀이 아닐경우 (처음과 팀이 변경됐을 경우)
        {
            SetTeamColor(team);
        }
        
        particleCollision.SetTeam(currentTeam);
        //파티클 충돌 스크립트로 넘겨줌
    }

    // 잉크 회복 (PlayerController에서 호출)
    public void RecoverInk()
    {
        if (currentInk < maxInk)
        {
            currentInk += inkRecoveryRate * Time.deltaTime;
            currentInk = Mathf.Min(currentInk, maxInk); // 최대치를 넘지 않도록
        }
    }

    private void SetTeamColor(Team team) //색 지정
    {
        currentTeam = team;
        //나의 팀 정보 
        Color teamColor = teamColorInfo.GetTeamColor(currentTeam);
        //팀정보를 토대로 팀색을 가져옴

        ///파티클에 팀컬러 지정
        mainParticleMain.startColor = teamColor;
        fireEffectMain.startColor = teamColor;
        //floorInkEffectMain.startColor = teamColor;
    }

    private void UpdateStartSpeed() ///
    {
        if (lastParticleSpeed != particleSpeed)
        {
            lastParticleSpeed = particleSpeed;
            startSpeedCurve.constantMin = particleSpeed;
            startSpeedCurve.constantMax = particleSpeed + 5f;
            mainParticleMain.startSpeed = startSpeedCurve;
        }
        
    }
}

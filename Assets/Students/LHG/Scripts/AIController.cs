using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.AI;


public class AIController : BaseController
{
    public MoveModule MoveModule { get; private set; }
    public FireModule FireModule { get; private set; }
    public DetectModule DetectModule { get; private set; }
    public AIStateMachine StateMachine { get; private set; }

    [Header("Set Values")]
    [SerializeField] public float fireInterval = 1; //발사간격(초)
    [SerializeField] public float detectInterval = 1f; //탐지간격(초)
    [SerializeField] public float detectRadius = 10f; //탐지 범위(m)

    /// <summary>
    /// 봇 바닥탐지 관련. player controller에서 참고함
    /// </summary>
    public Vector3 GroundNormal { get; private set; } = Vector3.up;
    public bool IsGrounded { get; private set; }
    public LayerMask groundLayer;
    public LayerMask inkableLayer;
    public InkStatus CurrentGroundInkStatus { get; private set; } = InkStatus.NONE;
    [SerializeField, Range(0.1f, 2.0f)]
    private float inkColorThreshold = 1.0f;
    [Range(0.1f, 1f)]
    public float enemyInkSpeedModifier = 0.5f;

    public bool canControl = false;/////////////

    //네브매쉬관련
    public NavMeshAgent agent { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (photonView.IsMine)
        {
            MineInit();
        }
        else
        {
            rig.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            MineAnimationProcess();
        }
        if (!photonView.IsMine && !IsDead)
        {
            OtherClientProcess();
        }
    }

    public override void OnEnable()/////////////////
    {
        base.OnEnable();
        GameManager.OnGameStarted += EnableControl;
        GameManager.OnGameEnded += DisableControl;
    }
    public override void OnDisable()///////////////
    {
        base.OnDisable();
        GameManager.OnGameStarted -= EnableControl;
        GameManager.OnGameEnded -= DisableControl;
    }

    private void EnableControl()///////////////
    {
        canControl = true;
        inkParticleGun.FireParticle(MyTeam, true);
    }

    private void DisableControl()///////////////
    {
        canControl = false;
        inkParticleGun.FireParticle(MyTeam, false);
        if (photonView.IsMine)
        {
            StopAllActions();
        }
    }

    private void Update()
    {
        if (photonView.IsMine) 
        {
            StateMachine.Update();
            GroundAndInkCheck();
            TestTeamSelection(); // TODO: 테스트코드 삭제할 것.
        }
        
    }

    void Start()
    {
       //// 맨 처음 게임 시작 시 원격은 Ink발사가 안되는 문제 해결
       //if (!photonView.IsMine)
       //{
       //    inkParticleGun.FireParticle(MyTeam, true);
       //}

        ReadyToPlay();

        //MyTeam = Team.Team1; //TODO : 임시 팀지정, 삭제할 것!!!!!!!!
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    private void MineInit() //포톤뷰가 본인일 때만 수행
    {
        //statedic대신 모듈화한 개별 스크립트로 관리
        MoveModule = new MoveModule(this);
        FireModule = new FireModule(this, weaponView);
        DetectModule = new DetectModule(this);

        StateMachine = gameObject.GetOrAddComponent<AIStateMachine>();

        //시작시 idle상태로
        StateMachine.SetState(new IdleState(this));

        teamColorInfo = FindObjectOfType<TeamColorInfo>();

        CurHp = MaxHp;
        IsDead = false;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void MineAnimationProcess()
    {
        if (humanAnimator != null)
        {
            // Move 관련 입력 처리
            humanAnimator.SetBool(IsMove, IsMoving);
            humanAnimator.SetFloat(MoveX, 0);

            if (IsMoving)
            {
                humanAnimator.SetFloat(MoveY, 1);
            }
            else
            {
                humanAnimator.SetFloat(MoveY, 0);
            }
            // 공격 중이면 공격 모션
            humanAnimator.SetBool(Fire, IsFiring);
        }
    }
    private void OtherClientProcess() // 원격일 경우의 처리
    {
        if (!IsDead && !humanModel.activeSelf) // 부활 시
        {
            humanModel.SetActive(true);
            col.enabled = true;
        }

        else if (!IsDead && humanModel.activeSelf) // 안죽었을 때
        {
            if (humanAnimator != null)
            {
                // 애니메이션 상태 파라매터 처리
                humanAnimator.SetBool(IsMove, IsMoving);

                // Move 관련 입력 처리
                humanAnimator.SetFloat(MoveX, networkMoveX);
                humanAnimator.SetFloat(MoveY, networkMoveY);

                // 공격 중이면 공격 모션
                humanAnimator.SetBool(Fire, IsFiring);
            }
            // 지연 보상
            deltaPos = Vector3.Distance(transform.position, networkPos);
            deltaRot = Quaternion.Angle(transform.rotation, networkRot);

            interpolatePos = deltaPos * Time.deltaTime * PhotonNetwork.SerializationRate;
            interpolateRot = deltaRot * Time.deltaTime * PhotonNetwork.SerializationRate;

            transform.position = Vector3.MoveTowards(transform.position, networkPos, interpolatePos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRot, interpolateRot);
        }
    }

    [PunRPC]
    public override void TakeDamage(float amount) // 무기에 의해서, 로컬만 호출됨
    {
        if (IsDead) return;

        CurHp -= amount;
        hitRoutine ??= StartCoroutine(HitRoutine());
        if (CurHp <= 0)
        {
            CurHp = 0;
            Debug.Log("AI 플레이어 죽음");

            photonView.RPC("AiDie", RpcTarget.All);
        }

        Debug.Log($"현재 체력{CurHp}");

    }

    [PunRPC]
    public void AiDie() // 전체 클라이언트에 호출됨. TakeDamage에서 호출
    {
        IsDead = true;
        IsDeadState = true;
        Instantiate(dieParticle, transform);
        if (photonView.IsMine)
        {
            if (CurHp <= 0)
            {
                //TODO hit애니메이션 + 딜레이
                StateMachine.SetState(new DeathState(this));

                Debug.Log("AI 사망");
            }
        }
        else
        {
            humanModel.SetActive(false);
            col.enabled = false;
        }
    }

    public void Respawn() // DeathState 상태에서 호출됨. 본인만 수행
    {
        humanModel.SetActive(true);
        col.enabled = true;

        //AI 리셋
        CurHp = MaxHp;
        IsDead = false;
        StateMachine.SetState(new IdleState(this));

        //리스폰 위치버그조치
        Transform[] spawnArray = MyTeam == Team.Team1 ? Manager.Game.team1SpawnPoints : Manager.Game.team2SpawnPoints;
        transform.position = spawnArray[0].position; //TODO 스폰어레이 0번에 넣는거..조금불안하긴함
        transform.rotation = spawnArray[0].rotation;

        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;

        Manager.Audio.PlayClip("Respawn", transform.position);

        Debug.Log("봇 리스폰 완료");
    }

    // TODO: 테스트 종료 후 삭제
    private void TestTeamSelection()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            MyTeam = Team.Team1;
            Debug.Log($"팀 변경: {MyTeam}");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            MyTeam = Team.Team2;
            Debug.Log($"팀 변경: {MyTeam}");
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //  transform 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext((int)MyTeam);

            // 사망 정보 전송
            stream.SendNext(IsDead);
            stream.SendNext(IsDeadState);

            // 애니메이션 전송
            stream.SendNext(IsMoving);
            stream.SendNext(humanAnimator.GetFloat(MoveX));
            stream.SendNext(humanAnimator.GetFloat(MoveY));
            stream.SendNext(humanAnimator.GetBool(Fire));

            // 얼굴 정보 전송
            stream.SendNext((int)faceType);


        }
        else if (stream.IsReading)
        {
            // transform 수신
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
            MyTeam = (Team)(int)stream.ReceiveNext();

            // 사망정보 수신
            IsDead = (bool)stream.ReceiveNext();
            IsDeadState = (bool)stream.ReceiveNext();

            // 애니메이션 수신
            IsMoving = (bool)stream.ReceiveNext();
            networkMoveX = (float)stream.ReceiveNext();
            networkMoveY = (float)stream.ReceiveNext();
            IsFiring = (bool)stream.ReceiveNext();

            // 얼굴 정보 수신
            FaceOff((FaceType)(int)stream.ReceiveNext());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            MoveModule.OnCollisionEnter(collision);
        }
    }


    private void GroundAndInkCheck()
    {
        LayerMask combinedLayer = groundLayer | inkableLayer;
        Vector3 groundRayStart = transform.position + Vector3.up * 0.1f;
        float groundRayDistance = 0.6f; // Raycast 길이를 안정적으로 수정

        Debug.DrawRay(groundRayStart, Vector3.down * groundRayDistance, Color.red);

        // 바닥 체크
        if (Physics.Raycast(groundRayStart, Vector3.down, out RaycastHit groundHit, groundRayDistance, combinedLayer))
        {
            IsGrounded = true;
            GroundNormal = groundHit.normal;
            if (groundHit.collider.TryGetComponent<PaintableObj>(out var paintableObj))
            {
                // 잉크 색상은 비동기로 읽어오므로, 이 값은 약간의 딜레이가 있을 수 있음
                SplatmapReader.ReadPixel(paintableObj.splatMap, groundHit.textureCoord, OnGroundColorRead);
            }
            else
            {
                CurrentGroundInkStatus = InkStatus.NONE;
            }
        }
        else
        {
            IsGrounded = false;
            GroundNormal = Vector3.up;
            CurrentGroundInkStatus = InkStatus.NONE;
        }
    }

    private void OnGroundColorRead(Color color)
    {
        CurrentGroundInkStatus = GetInkStatusFromColor(color);
        //Debug.Log($"<color=green>봇의 최종 바닥 잉크 상태: {CurrentGroundInkStatus}</color>");
    }

    private InkStatus GetInkStatusFromColor(Color color)
    {
        if (color.a < 0.2f) return InkStatus.NONE;

        Color myTeamInputColor = teamColorInfo.GetTeamInputColor(MyTeam);

        Team enemyTeam = (MyTeam == Team.Team1) ? Team.Team2 : Team.Team1;
        if (MyTeam == Team.None) enemyTeam = Team.None;
        Color enemyTeamInputColor = teamColorInfo.GetTeamInputColor(enemyTeam);

        float diffToMyTeam = Mathf.Abs(color.r - myTeamInputColor.r) + Mathf.Abs(color.g - myTeamInputColor.g) + Mathf.Abs(color.b - myTeamInputColor.b);
        float diffToEnemyTeam = Mathf.Abs(color.r - enemyTeamInputColor.r) + Mathf.Abs(color.g - enemyTeamInputColor.g) + Mathf.Abs(color.b - enemyTeamInputColor.b);

        if (diffToMyTeam < inkColorThreshold && diffToMyTeam < diffToEnemyTeam)
        {
            return InkStatus.OUR_TEAM;
        }
        else if (diffToEnemyTeam < inkColorThreshold && diffToEnemyTeam < diffToMyTeam)
        {
            return InkStatus.ENEMY_TEAM;
        }

        return InkStatus.NONE;
    }
    public void SetTeam(Team team)
    {
        MyTeam = team;

        // 팀 관련 설정 초기화
        if (teamColorInfo == null)
            teamColorInfo = FindObjectOfType<TeamColorInfo>();

        inkParticleGun?.FireParticle(MyTeam, true); // 잉크 색상 적용

        Debug.Log($"[AIController] 팀 설정됨: {MyTeam}");
    }

    private void StopAllActions()
    {
        MoveModule.StopWander();
        FireModule.StopFire();
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
    }

}

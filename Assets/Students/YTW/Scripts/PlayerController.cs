using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum InkStatus { NONE, OUR_TEAM, ENEMY_TEAM }

public class PlayerController : BaseController
{
    public StateMachine stateMachine { get; private set; }
    public Dictionary<HighState, PlayerState> highStateDic { get; private set; }

    public PlayerInput input;
    

    private float recenterCooldownTimer;
    //private const float RECENTER_COOLDOWN = 1.0f;

    [Header("카메라 설정")]
    public GameObject playerCameraObject;
    public ThirdPersonCamera tpsCamera;
    public Camera mainCamera;
    public Transform cameraPivot;
    public GameObject spectatorCamera;

    [Header("플레이어 설정")]
    [SerializeField] public Transform ModelTransform;
    public LayerMask groundLayer;
    public float maxSlopeAngle = 45f;
    public float humanJumpForce = 15f;
    public float squidJumpForce = 15f;
    public float squidSpeed = 8f;
    private Vector3 squidColCenter = new Vector3(0,0.5f,0);
    private Vector3 humanColCenter = new Vector3(0,1f,0);

    [Header("점프 설정")]
    public float gravityScale = 4f;
    public float fallingGravityScale = 7f;


    [Header("무기 설정")]
    public ParticleSystem weaponFireEffect;

    [Header("무기 Transform")]
    public Transform weaponTransform;
    public Transform muzzleTransform;

    [Header("잉크 상호작용 설정")]
    [Tooltip("잉크가 칠해질 수 있는 오브젝트의 레이어")]
    public LayerMask inkableLayer;
    [Tooltip("적 팀 잉크 위에서의 이동 속도 감소 배율")]
    [Range(0.1f, 1f)]
    public float enemyInkSpeedModifier = 0.5f;
    // 잉크 얼마나 잘 감지할지
    [SerializeField, Range(0.1f, 2.0f)]
    private float inkColorThreshold = 1.0f;
    [Tooltip("적 팀 잉크 위에 있을 때 초당 받는 데미지")]
    public float damagePerSecondOnEnemyInk = 10f;
    [Tooltip("아군 팀 잉크 위에 있을 때 초당 회복하는 체력")]
    public float healthPerSecondOnOurInk = 5f;

    // 잉크 감지 시스템 파라매터
    //private float groundCheckTimer = 0f;
    //private const float GROUND_CHECK_INTERVAL = 0.1f; // 1초에 10번 검사

    // 플레이어 이동 판정
    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;
    public InkStatus CurrentGroundInkStatus { get; private set; } = InkStatus.NONE;
    public InkStatus CurrentWallInkStatus { get; private set; } = InkStatus.NONE;
    public bool IsOnWalkableWall { get; private set; }
    public bool IsAtWallEdge { get; private set; }
    public bool IsVaulting;
    public Vector3 WallNormal { get; private set; } = Vector3.zero;

    
    // 플레이어 루프 사운드
    public AudioSource squidSwim;
    public AudioSource squidSwimBubble;
    
    protected override void Awake()
    {

        base.Awake();
        squidAnimator = squidModel.GetComponentInChildren<Animator>();
        CurHp = MaxHp;

        if (photonView.IsMine)
        {
            MineInit();
        }

        else if (!photonView.IsMine)
        {
            // 이 캐릭터가 다른 플레이어(원격)의 것이므로, 이 캐릭터에 포함된 카메라를 비활성화합니다.
            // 이렇게 해야 다른 플레이어의 카메라가 내 화면에 그려지거나 컨트롤을 방해하는 문제를 막을 수 있습니다.
            if (playerCameraObject != null)
            {
                playerCameraObject.SetActive(false);
            }

            rig.isKinematic = true;
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine && stateMachine != null)
        {
            stateMachine.FixedUpdate();
            if (!IsGrounded && rig.useGravity && !IsVaulting)
            {
                if (rig.velocity.y >= 0)
                {
                    rig.velocity += Vector3.up * (Physics.gravity.y * (gravityScale - 1) * Time.fixedDeltaTime);
                }
                else
                {
                    rig.velocity += Vector3.up * (Physics.gravity.y * (fallingGravityScale - 1) * Time.fixedDeltaTime);
                }
            }

            if (humanAnimator.GetBool(Fire) != IsFiring)
            {
                humanAnimator.SetBool(Fire, IsFiring);
            }
            
            if (IsFiring && hitRoutine == null)
            {
                FaceOff(FaceType.Upset);
            }
            else if (!IsFiring && hitRoutine == null)
            {
                FaceOff(FaceType.Idle);
            }
        }

        else if (!photonView.IsMine && !IsDead)
        {
            OthersAnimation();
        }
    }

    void Update()
    {
        // 본인의 photonView일 경우
        if (photonView.IsMine)
        {
            if (recenterCooldownTimer > 0)
            {
                recenterCooldownTimer -= Time.deltaTime;
            }

            GroundAndInkCheck();
            HandleInkAndHealth();

            if (stateMachine != null)
            {
                stateMachine.Update();
            }

            HandleTeamSelection();

            if (input.IsRecenterPressed && recenterCooldownTimer <= 0f)
            {
                if (tpsCamera != null)
                {
                    tpsCamera.Recenter(transform.forward);
                }
            }
        }
        // 본인의 photonView가 아닐 경우
        else
        {
            // 리스폰 조건 체크
            if ( IsDead && !IsDeadState)
            {
                Respawn();
            }
        }
    }

    void LateUpdate()
    {
        if (photonView.IsMine && tpsCamera != null)
        {
            tpsCamera.CameraUpdate(input.MouseInput.x, input.MouseInput.y);

        }

        if (inkParticleGun.isFiring && !fireSound.isPlaying)
        {
            fireSound.Play();
        }
        else if (!inkParticleGun.isFiring && fireSound.isPlaying)
        {
            fireSound.Stop();
        }
    }

    public void MineInit()
    {
        input = GetComponent<PlayerInput>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        stateMachine = new StateMachine();
        highStateDic = new Dictionary<HighState, PlayerState>();

        highStateDic.Add(HighState.HumanForm, new Player_Human(this, stateMachine));
        highStateDic.Add(HighState.SquidForm, new Player_Squid(this, stateMachine));
        highStateDic.Add(HighState.Die, new Player_Die(this, stateMachine));

        stateMachine.Initialize(highStateDic[HighState.HumanForm]);

        // 게임 매니저에 팀 할당
        // TODO : 테스트 끝나면 해제
        StartCoroutine(WaitForTeamAssignment());

        // 카메라 동기화
        if (playerCameraObject == null)
        {
            Debug.LogError("Player Camera Object가 Inspector에 할당되지 않았습니다.", this);
            return;
        }
        playerCameraObject.SetActive(true);

        // 메인 카메라 컴포넌트를 찾습니다.
        mainCamera = playerCameraObject.GetComponentInChildren<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Player Camera Object 또는 그 자식에서 Camera 컴포넌트를 찾을 수 없습니다", this);
        }

        if (tpsCamera == null)
        {
            tpsCamera = playerCameraObject.GetComponent<ThirdPersonCamera>();
        }
        if (tpsCamera == null)
        {
            tpsCamera = playerCameraObject.GetComponentInParent<ThirdPersonCamera>();
        }

        if (tpsCamera != null)
        {
            tpsCamera.followTransform = this.cameraPivot;
        }
        else
        {
            Debug.LogError("ThirdPersonCamera 스크립트를 찾을 수 없습니다.", this);
        }

        if (spectatorCamera == null)
        {
            spectatorCamera = GameObject.FindWithTag("SpectatorCamera");
            if (spectatorCamera != null) spectatorCamera.SetActive(false); // 처음엔 비활성화
        }
    }

    private void OthersAnimation() // 로컬 포톤뷰가 아닌 플레이어들 설정
    {
        // 사운드 체크
        HandleSquidSound();
        
        // 오징어 폼
        if (isSquidNetworked)
        {
            if (humanModel.activeSelf)
            {
                humanModel.SetActive(false);
            }

            if (!squidModel.activeSelf)
            {
                squidModel.SetActive(true);
            }

            col.direction = 2;
            col.center = squidColCenter;
            col.height = 0.5f;
            col.radius = 0.45f;

            if (squidAnimator != null)
            {
                squidAnimator.SetBool(IsMove,IsMoving);
                squidAnimator.SetBool(IsAir,isAir);
                squidAnimator.SetFloat(MoveSpeed, networkMoveSpeed);

                if (isAir && !isJump)
                {
                    isJump = true;
                    squidAnimator.SetTrigger(JumpTrigger);
                }
                else if (!isAir && isJump)
                {
                    isJump = false;
                }
            }
        }
        // 인간 폼
        else
        {
            humanModel.SetActive(true);
            squidModel.SetActive(false);

            col.direction = 1;
            col.center = humanColCenter;
            col.height = 2.0f;
            col.radius = 0.5f;

            if (humanAnimator != null)
            {
                // 애니메이션 상태 파라매터 처리
                humanAnimator.SetBool(IsMove,IsMoving);
                humanAnimator.SetBool(IsAir,isAir);
                // TODO: 공격 시 애니메이션 처리 IsFiring
                // 점프 애니메이션 트리거 파라매터 처리
                if (isAir && !isJump)
                {
                    humanAnimator.SetTrigger(JumpTrigger);
                    isJump = true;
                }
                else if (!isAir && isJump)
                {
                    isJump = false;
                }

                // Move 관련 입력 처리
                humanAnimator.SetFloat(MoveX, networkMoveX);
                humanAnimator.SetFloat(MoveY, networkMoveY);
            }
        }

        // 지연 보상
        deltaPos = Vector3.Distance(transform.position, networkPos);
        deltaRot = Quaternion.Angle(transform.rotation, networkRot);
        deltaModelRot = Quaternion.Angle(ModelTransform.rotation, networkModelRot);

        interpolatePos = deltaPos * Time.deltaTime * PhotonNetwork.SerializationRate;
        interpolateRot = deltaRot * Time.deltaTime * PhotonNetwork.SerializationRate;
        interpolateModelRot = deltaModelRot * Time.deltaTime * PhotonNetwork.SerializationRate;

        transform.position = Vector3.MoveTowards(transform.position, networkPos, interpolatePos);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRot, interpolateRot);
        ModelTransform.rotation = Quaternion.RotateTowards(ModelTransform.rotation, networkModelRot, interpolateModelRot);
    }

    private IEnumerator WaitForTeamAssignment()
    {
        Debug.Log("[PlayerController] 팀 할당 대기 시작");

        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("team"))
        {
            Debug.Log("[PlayerController] team 속성 대기 중...");
            yield return null;
        }

        string teamString = PhotonNetwork.LocalPlayer.CustomProperties["team"].ToString();
        Debug.Log($"[PlayerController] 받은 team 값: {teamString}");

        if (teamString == "Team1") MyTeam = Team.Team1;
        else if (teamString == "Team2") MyTeam = Team.Team2;
        else MyTeam = Team.None;

        Debug.Log($"[PlayerController] 팀 할당 완료: {MyTeam}");
    }

    private void GroundAndInkCheck()
    {
        // 바닥 체크 ray
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

        Vector3 wallDirection = ModelTransform.forward;
        Vector3 wallRayStart = transform.position + transform.up * (col.height / 2);
        Vector3 edgeRayStart = transform.position + transform.up * (col.height - 0.1f);
        float wallRayDistance = 1.2f;
        Debug.DrawRay(wallRayStart, wallDirection * wallRayDistance, Color.blue);

        // 벽 체크 ray
        if (Physics.Raycast(wallRayStart, wallDirection, out RaycastHit wallHit, wallRayDistance, inkableLayer))
        {
            // 부딪힌 표면의 경사각을 계산
            float surfaceAngle = Vector3.Angle(Vector3.up, wallHit.normal);

            // 경사각이 설정된 maxSlopeAngle보다 클 때만 벽으로 취급
            if (surfaceAngle > maxSlopeAngle)
            {
                WallNormal = wallHit.normal;
                if (wallHit.collider.TryGetComponent<PaintableObj>(out var paintableObj))
                {
                    SplatmapReader.ReadPixel(paintableObj.splatMap, wallHit.textureCoord, OnWallColorRead);

                    if (IsOnWalkableWall && !Physics.Raycast(edgeRayStart, wallDirection, wallRayDistance, inkableLayer))
                    {
                        IsAtWallEdge = true;
                    }
                    else
                    {
                        IsAtWallEdge = false;
                    }
                }
                else
                {
                    CurrentWallInkStatus = InkStatus.NONE;
                    IsOnWalkableWall = false;
                    IsAtWallEdge = false;
                }
            }
            else
            {
                WallNormal = Vector3.zero;
                CurrentWallInkStatus = InkStatus.NONE;
                IsOnWalkableWall = false;
                IsAtWallEdge = false;
            }
        }
        else 
        {
            WallNormal = Vector3.zero;
            CurrentWallInkStatus = InkStatus.NONE;
            IsOnWalkableWall = false;
            IsAtWallEdge = false;
        }
    }


    private void OnGroundColorRead(Color color)
    {
        // Debug.Log($"<color=yellow>읽어온 바닥 색상 (RGBA): ({color.r}, {color.g}, {color.b}, {color.a})</color>");
        CurrentGroundInkStatus = GetInkStatusFromColor(color);
        // Debug.Log($"<color=green>최종 바닥 잉크 상태: {CurrentGroundInkStatus}</color>");
    }

    private void OnWallColorRead(Color color)
    {
        CurrentWallInkStatus = GetInkStatusFromColor(color);
        if (stateMachine.CurrentState != highStateDic[HighState.SquidForm])
        {
            IsOnWalkableWall = false;
            return;
        }

        IsOnWalkableWall = (CurrentWallInkStatus == InkStatus.OUR_TEAM);
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

    //public void LookAround()
    //{
    //    Vector3 playerRotation = transform.eulerAngles;
    //    playerRotation.y = tpsCamera.yRotation;
    //    transform.eulerAngles = playerRotation;
    //}

    // TODO : test용
    private void HandleTeamSelection()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MyTeam = Team.Team1;
            Debug.Log($"팀 변경: {MyTeam}");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            MyTeam = Team.Team2;
            Debug.Log($"팀 변경: {MyTeam}");
        }
    }

    [PunRPC]
    public override void TakeDamage(float amount) //InkParticleCollision에 의해서 들어옴 로컬만 수행
    {
        if (IsDead) return;

        CurHp -= amount;
        hitRoutine ??= StartCoroutine(HitRoutine());
        Debug.Log($"현재 체력{CurHp}");

        if (CurHp <= 0)
        {
            CurHp = 0;
            IsDead = true;
            Debug.Log("플레이어 죽음");

            photonView.RPC("PlayerDie", RpcTarget.All);
        }
    }

    [PunRPC]
    public void PlayerDie() // TakeDamage의 조건에 따라서 들어옴. 전역 수행
    {
        Manager.Audio.PlayClip("Dead",transform.position);
        Instantiate(dieParticle, transform);
        // 원격으로도 죽은 처리 해줘야 함
        IsDead = true;
        IsDeadState = true;
        // 상태 머신을 Die 상태로 변경
        if (photonView.IsMine)
        {
            stateMachine.ChangeState(highStateDic[HighState.Die]);
        }
        else
        {
            Debug.Log("본인 PhotonView 아님. Death 처리");
            // 본인의 PhotonView가 아닐 경우
            col.enabled = false;
            humanModel.SetActive(false);
            squidModel.SetActive(false);
        }
    }

    public void Respawn()
    {
        // 체력 초기화
        CurHp = MaxHp;

        if (!photonView.IsMine)
        {
            IsDead = false;
            col.enabled = true;
            humanModel.SetActive(true);
            return;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("team", out object teamValue))
        {
            string team = teamValue.ToString();
            // GameManager 인스턴스에서 팀별 스폰 포인트 배열을 가져옴
            Transform[] spawnPoints = (team == "Team1")
                ? Manager.Game.team1SpawnPoints
                : Manager.Game.team2SpawnPoints;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // ActorNumber를 사용하여 이 플레이어의 고유 스폰 위치를 결정
                int actorIndex = PhotonNetwork.LocalPlayer.ActorNumber;
                Transform spawnPoint = spawnPoints[actorIndex % spawnPoints.Length];

                // 위치 및 회전 초기화
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;

                // 물리 상태 초기화 (순간이동 후 잔여 속도 제거)
                rig.velocity = Vector3.zero;
                rig.angularVelocity = Vector3.zero;
            }
            else
            {
                Debug.LogError($"팀 '{team}'에 대한 스폰 포인트가 설정되지 않았거나 비어 있습니다! 기본 위치로 리스폰합니다.");
                transform.position = new Vector3(0, 5, 0);
                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            Debug.LogError("플레이어의 팀 정보를 찾을 수 없어 리스폰 위치를 결정할 수 없습니다. 기본 위치로 리스폰합니다.");
            transform.position = new Vector3(0, 5, 0);
            transform.rotation = Quaternion.identity;
        }

        Manager.Audio.PlayClip("Respawn", transform.position);
        stateMachine.ChangeState(highStateDic[HighState.HumanForm]);
        Debug.Log("플레이어 리스폰");
    }

    // PUN2가 주기적으로 호출하여 데이터를 동기화하는 콜백 함수
    // OnPhotonSerializeView : 정기적으로 데이터를 주고받는 통신 채널
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // stream : 데이터통로, info 메시지에대한 추가정보
    {
        // 내가 직접 조종하는 캐릭터에서만 작동
        if (stream.IsWriting)
        {

            // 0 플레이어 사망 정보 전송
            stream.SendNext(IsDeadState);

            if (stateMachine == null || highStateDic == null)
            {
                Debug.LogError("스테이트 머신 또는 스테이트 딕셔너리 없음");
                return;
            }

            // 1 현재 내 위치를 stream에 실림
            stream.SendNext(transform.position);
            // 2 현재 내 회전값을 stream에 실림
            stream.SendNext(transform.rotation);

            // 3 팀정보 전송
            stream.SendNext((int)MyTeam);
            
            // 현재 내 상태가 오징어 폼인지 아닌지(bool)를 stream에 실림
            // (stateMachine.CurrentState가 SquidForm 상태와 같으면 true, 아니면 false가 실림)
            if (stateMachine != null && highStateDic != null && highStateDic.ContainsKey(HighState.SquidForm))
            {
                // 4 오징어 상태 여부
                stream.SendNext(stateMachine.CurrentState == highStateDic[HighState.SquidForm]);
            }
            else
            {
                // 4 아닐경우 false
                stream.SendNext(false);
            }

            // 인간폼일때 Send
            if (stateMachine.CurrentState == highStateDic[HighState.HumanForm])
            {
                // 5현재 움직이는 여부 전송
                stream.SendNext(humanAnimator.GetBool(IsMove));
                // 6현재 공중인지 전송
                stream.SendNext(humanAnimator.GetBool(IsAir));
                // 7,8 이동 파라매터
                stream.SendNext(humanAnimator.GetFloat(MoveX));
                stream.SendNext(humanAnimator.GetFloat(MoveY));
                // 9 현재 얼굴
                stream.SendNext((int)faceType);
                // 10 공격 여부 전송
                stream.SendNext(IsFiring);
                // 11 모델 회전 전송
                stream.SendNext(ModelTransform.rotation);
            }
            // 오징어 폼일때 Send
            else if (stateMachine.CurrentState == highStateDic[HighState.SquidForm])
            {
                stream.SendNext(squidAnimator.GetFloat(MoveSpeed));
                stream.SendNext(squidAnimator.GetBool(IsMove));
                stream.SendNext(squidAnimator.GetBool(IsAir));
            }

        }
        else if (stream.IsReading)
        // stream.IsWriting이 false, 즉 stream.IsReading일 때
        // 다른 사람의 컴퓨터에 보이는 내 캐릭터, 또는 내 컴퓨터에 보이는 다른 사람의 캐릭터에서 작동
        {
            // 0 플레이어 사망정보 수신
            IsDeadState = (bool)stream.ReceiveNext();
            // 1 위치
            networkPos = (Vector3)stream.ReceiveNext();
            // 2 회전
            networkRot = (Quaternion)stream.ReceiveNext();
            // 3 팀 정보
            MyTeam= (Team)(int)stream.ReceiveNext(); // TODO: 팀변경 테스트 끝나면 지우기
            // 4 오징어 여부
            isSquidNetworked = (bool)stream.ReceiveNext();

            // 인간 폼일때 수신
            if (!isSquidNetworked && !IsDeadState)
            {
                // 5 움직이는지
                IsMoving =  (bool)stream.ReceiveNext();
                // 6 공중인지
                isAir = (bool)stream.ReceiveNext();
                // 7,8 이동 파라매터 
                networkMoveX = (float)stream.ReceiveNext();
                networkMoveY = (float)stream.ReceiveNext();
                // 9 현재 얼굴
                FaceOff((FaceType)(int)stream.ReceiveNext());
                // 10 공격 상태 수신
                FireStatus((bool)stream.ReceiveNext());
                // 11 모델 회전 수신
                networkModelRot = (Quaternion)stream.ReceiveNext();
            }
            // 오징어 폼일때 수신
            else if (isSquidNetworked && !IsDeadState)
            {
                networkMoveSpeed = (float)stream.ReceiveNext();
                IsMoving = (bool)stream.ReceiveNext();
                isAir = (bool)stream.ReceiveNext();
            }

        }
    }

    private void FireStatus(bool isFire)
    {
        if (IsFiring == isFire) return;
        IsFiring = isFire;
        humanAnimator.SetBool(Fire,isFire);
    }

    public void ChangeHighState(HighState state)
    {
        if (highStateDic.ContainsKey(state))
        {
            stateMachine.ChangeState(highStateDic[state]);
        }
    }



    public void SetModelVisibility(GameObject model, bool isVisible)
    {
        if (model == null) return;

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (weaponFireEffect != null && renderer.gameObject == weaponFireEffect.gameObject)
            {
                continue;
            }

            renderer.enabled = isVisible;
        }

        Animator animator = model.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = isVisible;
        }
    }

    private void HandleInkAndHealth()
    {
        // 땅에 있지 않다면 아무것도 하지 않음
        if (!IsGrounded) return;

        switch (CurrentGroundInkStatus)
        {
            case InkStatus.OUR_TEAM:
                // 체력 회복
                if (CurHp < MaxHp)
                {
                    // 이전 체력 기록
                    float oldHp = CurHp;

                    CurHp += healthPerSecondOnOurInk * Time.deltaTime;
                    CurHp = Mathf.Min(CurHp, MaxHp); // 최대 체력을 넘지 않도록 보정

                    // 체력이 실제로 회복되었을 때만 로그를 출력
                    if (CurHp > oldHp)
                    {
                        Debug.Log($"아군 잉크 위에서 체력 회복 HP: {CurHp:F1}");
                    }
                }
                break;

            case InkStatus.ENEMY_TEAM:
                // 데미지
                if (stateMachine.CurrentState == highStateDic[HighState.HumanForm] && CurHp > 0)
                {
                    Debug.Log($"적군 잉크 위에서 데미지를 입습니다");
                    TakeDamage(damagePerSecondOnEnemyInk * Time.deltaTime);
                }
                break;

            case InkStatus.NONE:
                // 잉크가 없는 곳에서는 아무 효과 없음
                break;
        }
    }

    private void HandleSquidSound()
    {
        // 소리 설정
        if (isSquidNetworked)
        {
            if (!squidSwim)
            {
                squidSwim = Manager.Audio.PlayClip("Swim", transform.position);
                squidSwim.transform.SetParent(transform);
                squidSwim.volume = 0.5f;
            }
            if (!squidSwimBubble)
            {
                squidSwimBubble = Manager.Audio.PlayClip("Bubble", transform.position);
                squidSwimBubble.transform.SetParent(transform);
                squidSwimBubble.volume = 0.5f;
            }

            if (!squidSwim.isPlaying)
            {
                squidSwim.Play();
            }

            if (!squidSwimBubble.isPlaying)
            {
                squidSwimBubble.Play();
            }
        }
        else
        {
            if (squidSwim && squidSwim.isPlaying)
            {
                squidSwim.Stop();
            }

            if (squidSwimBubble && squidSwimBubble.isPlaying)
            {
                squidSwimBubble.Stop();
            }
        }
    }
}

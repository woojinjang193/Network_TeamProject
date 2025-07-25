using Photon.Pun;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public enum InkStatus { NONE, OUR_TEAM, ENEMY_TEAM }

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{

    public StateMachine stateMachine { get; private set; }
    public Dictionary<HighState, PlayerState> highStateDic { get; private set; }

    public Animator humanAnimator;
    public Animator squidAnimator;

    public Rigidbody rig;
    public PlayerInput input;
    public CapsuleCollider col;

    private float recenterCooldownTimer = 0f;
    private const float RECENTER_COOLDOWN = 1.0f;

    [Header("카메라 설정")]
    public ThirdPersonCamera tpsCamera;
    public Camera mainCamera;
    public Transform cameraPivot;

    [Header("모델 설정")]
    public GameObject humanModel;
    public GameObject squidModel;
    public SkinnedMeshRenderer playerRenderer;
    public SkinnedMeshRenderer squidRenderer;

    [Header("플레이어 설정")]
    public LayerMask groundLayer;
    public float moveSpeed = 5f;
    public float humanJumpForce = 15f;
    public float squidJumpForce = 15f;
    public float squidSpeed = 8f;

    [Header("점프 설정")]
    public float gravityScale = 4f;
    public float fallingGravityScale = 7f;

    [Header("무기 설정")]
    public InkParticleGun inkParticleGun;
    public PhotonView weaponView;

    [Header("팀 설정")]
    private TeamColorInfo teamColorInfo;
    public Team myTeam { get; private set; } = Team.None;

    [Header("잉크 상호작용 설정")]
    [Tooltip("잉크가 칠해질 수 있는 오브젝트의 레이어")]
    public LayerMask inkableLayer;
    [Tooltip("적 팀 잉크 위에서의 이동 속도 감소 배율")]
    [Range(0.1f, 1f)]
    public float enemyInkSpeedModifier = 0.5f;

    // 네트워크
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private bool isSquidNetworked = false;
    private float networkMoveX = 0f;
    private float networkMoveZ = 0f;

    // 잉크 감지 시스템
    public bool IsGrounded { get; private set; } = false;
    public InkStatus CurrentGroundInkStatus { get; private set; } = InkStatus.NONE;
    public InkStatus CurrentWallInkStatus { get; private set; } = InkStatus.NONE;
    public bool IsOnWalkableWall { get; private set; } = false;
    public Vector3 WallNormal { get; private set; } = Vector3.zero;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        col = GetComponent<CapsuleCollider>();

        humanAnimator = humanModel.GetComponentInChildren<Animator>();
        squidAnimator = squidModel.GetComponentInChildren<Animator>();

        teamColorInfo = FindObjectOfType<TeamColorInfo>();

        playerRenderer = humanModel.GetComponent<SkinnedMeshRenderer>();
        squidRenderer = squidModel.GetComponent<SkinnedMeshRenderer>();

        if (photonView.IsMine)
        {
            mainCamera = Camera.main;
            tpsCamera = Camera.main.GetComponentInParent<ThirdPersonCamera>();
            tpsCamera.followTransform = this.cameraPivot;
            Init();
        }
        else
        {
            rig.isKinematic = true;
        }

    }

    void Start()
    {
        if (inkParticleGun != null)
        {
            weaponView = inkParticleGun.GetComponent<PhotonView>();
        }

        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Init()
    {
        stateMachine = new StateMachine();
        highStateDic = new Dictionary<HighState, PlayerState>();


        highStateDic.Add(HighState.HumanForm, new Player_Human(this, stateMachine));
        highStateDic.Add(HighState.SquidForm, new Player_Squid(this, stateMachine));
        // highStateDic.Add(HighState.Die, new Player_Die(this, stateMachine));       
        stateMachine.Initialize(highStateDic[HighState.HumanForm]);
    }

    void Update()
    {
        if (recenterCooldownTimer > 0)
        {
            recenterCooldownTimer -= Time.deltaTime;
        }


        if (photonView.IsMine)
        {
            GroundAndInkCheck();

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
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10.0f);

            humanModel.SetActive(!isSquidNetworked);
            squidModel.SetActive(isSquidNetworked);

            if (humanAnimator != null)
            {
                humanAnimator.SetFloat("moveX", networkMoveX);
                humanAnimator.SetFloat("moveZ", networkMoveZ);
            }
        }

        
        UpdatePlayerColor();
    }

    private void GroundAndInkCheck()
    {
        LayerMask combinedLayer = groundLayer | inkableLayer;
        Vector3 groundRayStart = transform.position + Vector3.up * 0.1f;
        float groundRayDistance = 0.3f; // Raycast 길이를 안정적으로 수정

        Debug.DrawRay(groundRayStart, Vector3.down * groundRayDistance, Color.red);

        // 바닥 체크
        if (Physics.Raycast(groundRayStart, Vector3.down, out RaycastHit groundHit, groundRayDistance, combinedLayer))
        {
            IsGrounded = true;
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
            CurrentGroundInkStatus = InkStatus.NONE;
        }

        // 벽 체크
        Vector3 wallRayStart = transform.position + Vector3.up * 0.5f;
        float wallRayDistance = 1f;
        Debug.DrawRay(wallRayStart, transform.forward * wallRayDistance, Color.blue);

        if (Physics.Raycast(wallRayStart, transform.forward, out RaycastHit wallHit, wallRayDistance, inkableLayer))
        {
            if (wallHit.collider.TryGetComponent<PaintableObj>(out var paintableObj))
            {
                SplatmapReader.ReadPixel(paintableObj.splatMap, wallHit.textureCoord, OnWallColorRead);
            }
            else
            {
                CurrentWallInkStatus = InkStatus.NONE;
                IsOnWalkableWall = false;
            }
        }
        else
        {
            CurrentWallInkStatus = InkStatus.NONE;
            IsOnWalkableWall = false;
        }
    }


    private void OnGroundColorRead(Color color)
    {
        Debug.Log($"<color=yellow>읽어온 바닥 색상 (RGBA): ({color.r}, {color.g}, {color.b}, {color.a})</color>");
        CurrentGroundInkStatus = GetInkStatusFromColor(color);
        Debug.Log($"<color=green>최종 바닥 잉크 상태: {CurrentGroundInkStatus}</color>");
    }

    private void OnWallColorRead(Color color)
    {
        CurrentWallInkStatus = GetInkStatusFromColor(color);
        IsOnWalkableWall = (CurrentWallInkStatus == InkStatus.OUR_TEAM);
    }

    private InkStatus GetInkStatusFromColor(Color color)
    {
        if (color.a < 0.1f) return InkStatus.NONE;

        Color myTeamInputColor = teamColorInfo.GetTeamInputColor(myTeam);

        Team enemyTeam = (myTeam == Team.Team1) ? Team.Team2 : Team.Team1;
        if (myTeam == Team.None) enemyTeam = Team.None;
        Color enemyTeamInputColor = teamColorInfo.GetTeamInputColor(enemyTeam);

        float diffToMyTeam = Mathf.Abs(color.r - myTeamInputColor.r) + Mathf.Abs(color.g - myTeamInputColor.g) + Mathf.Abs(color.b - myTeamInputColor.b);
        float diffToEnemyTeam = Mathf.Abs(color.r - enemyTeamInputColor.r) + Mathf.Abs(color.g - enemyTeamInputColor.g) + Mathf.Abs(color.b - enemyTeamInputColor.b);

        // 더 가까운 색상의 팀으로 판정하되, 색상 차이가 일정 임계값(0.1f)보다 작아야 함
        if (diffToMyTeam < 0.1f && diffToMyTeam < diffToEnemyTeam)
        {
            return InkStatus.OUR_TEAM;
        }
        else if (diffToEnemyTeam < 0.1f && diffToEnemyTeam < diffToMyTeam)
        {
            return InkStatus.ENEMY_TEAM;
        }

        return InkStatus.NONE;
    }


    void LateUpdate()
    {
        if (photonView.IsMine && tpsCamera != null)
        {
            tpsCamera.CameraUpdate(input.MouseInput.x, input.MouseInput.y);

        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine && stateMachine != null)
        {
            stateMachine.FixedUpdate();
        }
    }

    public void LookAround()
    {
        Vector3 playerRotation = transform.eulerAngles;
        playerRotation.y = tpsCamera.yRotation;
        transform.eulerAngles = playerRotation;
    }

    // test용
    private void HandleTeamSelection()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            myTeam = Team.Team1;
            Debug.Log($"팀 변경: {myTeam}");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            myTeam = Team.Team2;
            Debug.Log($"팀 변경: {myTeam}");
        }
    }

    private void UpdatePlayerColor()
    {
        if (teamColorInfo != null)
        {
            Color teamColor = teamColorInfo.GetTeamColor(myTeam);

            if (playerRenderer != null)
            {
                playerRenderer.material.color = teamColor;
            }

            if (squidRenderer != null)
            {
                squidRenderer.material.color = teamColor;
            }
        }
    }



    // PUN2가 주기적으로 호출하여 데이터를 동기화하는 콜백 함수
    // OnPhotonSerializeView : 정기적으로 데이터를 주고받는 통신 채널
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // stream : 데이터통로, info 메시지에대한 추가정보
    {
        // 내가 직접 조종하는 캐릭터에서만 작동
        if (stream.IsWriting)
        {
            // 현재 내 위치를 stream에 실림
            stream.SendNext(transform.position);
            // 현재 내 회전값을 stream에 실림
            stream.SendNext(transform.rotation);
            stream.SendNext((int)myTeam);
            // 현재 내 상태가 오징어 폼인지 아닌지(bool)를 stream에 실림
            // (stateMachine.CurrentState가 SquidForm 상태와 같으면 true, 아니면 false가 실림)
            if (stateMachine != null && highStateDic != null && highStateDic.ContainsKey(HighState.SquidForm))
            {
                stream.SendNext(stateMachine.CurrentState == highStateDic[HighState.SquidForm]);
            }
            else
            {
                stream.SendNext(false); 
            }

            if (humanAnimator != null)
            {
                stream.SendNext(humanAnimator.GetFloat("moveX"));
                stream.SendNext(humanAnimator.GetFloat("moveZ"));
            }
            else
            {
                stream.SendNext(0f);
                stream.SendNext(0f);
            }
        }
        else
        // stream.IsWriting이 false, 즉 stream.IsReading일 때
        // 다른 사람의 컴퓨터에 보이는 내 캐릭터, 또는 내 컴퓨터에 보이는 다른 사람의 캐릭터에서 작동
        {
            // 데이터 통로(stream)에서 첫 번째 데이터를 꺼내 networkPosition 변수에 저장
            this.networkPosition = (Vector3)stream.ReceiveNext();
            // 두 번째 데이터를 꺼내 networkRotation 변수에 저장
            this.networkRotation = (Quaternion)stream.ReceiveNext();
            // 세 번째 데이터를 꺼내 isSquidNetworked 변수에 저장
            this.myTeam = (Team)(int)stream.ReceiveNext();
            this.isSquidNetworked = (bool)stream.ReceiveNext();
            this.networkMoveX = (float)stream.ReceiveNext();
            this.networkMoveZ = (float)stream.ReceiveNext();
        }
    }

    public void ChangeHighState(HighState state)
    {
        if (highStateDic.ContainsKey(state))
        {
            stateMachine.ChangeState(highStateDic[state]);
        }
    }
}

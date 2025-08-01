using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class BaseController : MonoBehaviourPunCallbacks, IPunObservable
{
    // 애니메이터 해시
    protected static readonly int IsMove = Animator.StringToHash("IsMove");
    protected static readonly int MoveX = Animator.StringToHash("MoveX");
    protected static readonly int MoveY = Animator.StringToHash("MoveY");
    protected static readonly int IsAir = Animator.StringToHash("IsAir");
    protected static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    protected static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    
    [Header("플레이어 모델")]
    public GameObject humanModel;
    public GameObject squidModel;
    public Animator humanAnimator;
    public Animator squidAnimator;
    
    [Header("값 설정")]
    [SerializeField] public float moveSpeed = 5;
    public const float RESPAWN_TIME = 3.0f; // 리스폰까지 걸리는 시간
    
    [Header("팀 설정")]
    protected TeamColorInfo teamColorInfo;
    
    [Header("잉크 파티클 총")]
    public InkParticleGun inkParticleGun;
    public PhotonView weaponView;
    
    // 팀 설정
    private Team myTeam = Team.None;
    public Team MyTeam
    {
        get { return myTeam; }
        set { myTeam = value; }
    }
    
    // 컴포넌트 참조
    public Rigidbody rig;
    public CapsuleCollider col;
    
    // 네트워크 파라매터
    protected float networkMoveX;
    protected float networkMoveY;
    protected Vector3 networkPos;
    protected Quaternion networkRot;
    protected float deltaPos;
    protected float deltaRot;
    protected float interpolatePos;
    protected float interpolateRot;
    protected bool isSquidNetworked;
    protected bool isAir;
    protected bool isMove;
    public bool IsMoving
    {
        get{return isMove;}
        set { isMove = value; }
    }
    protected bool isJump;
    protected float networkMoveSpeed;
    
    //플레이어 체력 및 사망 파라매터
    private float curHp;
    public float CurHp
    {
        get { return curHp;}
        set { curHp = value; }
    }

    private float maxHp = 100f;
    public float MaxHp
    {
        get { return maxHp; }
        set { maxHp = value; }
    }

    private bool isDead;
    public bool IsDead
    {
        get { return isDead;}
        set { isDead = value; }
    }

    private bool isDeadState;
    public bool IsDeadState
    {
        get { return isDeadState; }
        set { isDeadState = value; }
    }

    protected virtual void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        rig = GetComponent<Rigidbody>();
        humanAnimator = humanModel.GetComponent<Animator>();
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        if (inkParticleGun != null)
        {
            weaponView = inkParticleGun.GetComponent<PhotonView>();
        }
        
    }
    
    public abstract void TakeDamage(float amount);
    public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}

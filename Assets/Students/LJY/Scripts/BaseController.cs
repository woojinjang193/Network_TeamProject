using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Photon.Pun;
using UnityEngine;

public enum DeathCause ////// JWJ 추가
{
    PlayerAttack,
    BotAttck,
    Fall,
    EnemyInk
}
public abstract class BaseController : MonoBehaviourPunCallbacks, IPunObservable
{
    // 애니메이터 해시
    protected static readonly int IsMove = Animator.StringToHash("IsMove");
    protected static readonly int MoveX = Animator.StringToHash("MoveX");
    protected static readonly int MoveY = Animator.StringToHash("MoveY");
    protected static readonly int IsAir = Animator.StringToHash("IsAir");
    protected static readonly int JumpTrigger = Animator.StringToHash("JumpTrigger");
    protected static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    protected static readonly int Fire = Animator.StringToHash("FiringNow");
    private static readonly int HitTrigger = Animator.StringToHash("HitTrigger");
    
    [Header("파티클 프리팹 참조")]
    [SerializeField] protected ParticleSystem dieParticle;
    
    [Header("Debug- 플레이어 모델")]
    public GameObject humanModel;
    public GameObject squidModel;
    public Animator humanAnimator;
    public Animator squidAnimator;
    public HumanFace humanFace;
    
    [Header("값 설정")]
    [SerializeField] public float moveSpeed = 5;
    public const float RESPAWN_TIME = 10.0f; // 리스폰까지 걸리는 시간
    
    [Header("팀 설정")]
    protected TeamColorInfo teamColorInfo;
    
    [Header("잉크 파티클 총")]
    public InkParticleGun inkParticleGun;
    public PhotonView weaponView;

    public AudioSource fireSound { get; set; }

    // 네트워크 사운드 판별
    private bool fireSoundPlaying;
    
    // 팀 설정
    private Team myTeam = Team.None;
    public Team MyTeam
    {
        get { return myTeam; }
        set { myTeam = value; }
    }
    
    // 피격 시 설정
    protected Coroutine hitRoutine;
    private float hitRecoveryTimer = 0.4f;
    
    // 얼굴 타입. FaceOff 함수에 의해서 자동으로 변경됨
    public FaceType faceType;
    
    // 컴포넌트 참조
    public Rigidbody rig;
    public CapsuleCollider col;
    
    // 네트워크 파라매터
    private bool isMove;
    protected float networkMoveX;
    protected float networkMoveY;
    protected Vector3 networkPos;
    protected Quaternion networkRot;
    protected Quaternion networkModelRot;
    protected float deltaPos;
    protected float deltaRot;
    protected float deltaModelRot;
    protected float interpolatePos;
    protected float interpolateRot;
    protected float interpolateModelRot;
    protected bool isSquidNetworked;
    protected bool isAir;
    
    // 논리 파라매터
    public bool IsMoving
    {
        get{return isMove;}
        set { isMove = value; }
    }
    protected bool isJump;
    protected float networkMoveSpeed;
    public bool IsFiring { get; set; }
    
    
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

    protected string killerName = "";
    protected string victimName = "";
    protected DeathCause deathCause; 

    protected virtual void Awake()
    {
        humanFace = humanModel.GetComponent<HumanFace>();
        faceType = FaceType.Idle;
        col = GetComponent<CapsuleCollider>();
        rig = GetComponent<Rigidbody>();
        humanAnimator = humanModel.GetComponent<Animator>();
        teamColorInfo = FindObjectOfType<TeamColorInfo>();
        if (inkParticleGun != null)
        {
            weaponView = inkParticleGun.GetComponent<PhotonView>();
        }
        Manager.Game.RegisterPlayer(col, this);
        Manager.Audio.SetFireSound(this);
        if (!photonView.IsMine)
        {
            fireSound.volume = 0.5f;
        }
    }

    protected void ReadyToPlay() //봇이나 플레이어가 준비가 되면 마스터에게 알림
    {
        if (!photonView.IsMine) return;

        GameManager gameManager = FindObjectOfType<GameManager>();
        PhotonView gameManagerPhotonView = gameManager.GetComponent<PhotonView>();
        gameManagerPhotonView.RPC("NotifyCharSpawned", RpcTarget.MasterClient); 
    }
    
    public abstract void TakeDamage(float amount, PhotonMessageInfo info);
    public abstract void TakeDamage(float amount);
    public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
    
    public void FaceOff(FaceType type) // 얼굴 변경 함수. 로컬에 의해서만 호출됨. other는 알아서 변경됨
    {
        if (faceType == type) return; // 같은 얼굴이므로 return;
        for (int i = 0; i < humanFace.faceModel.Count; i++)
        {
            if (i == (int)type)
            {
                humanFace.faceModel[i].SetActive(true);
            }
            else
            {
                humanFace.faceModel[i].SetActive(false);
            }
        }
        faceType = type;
    }

    protected IEnumerator HitRoutine()
    {
        FaceOff(FaceType.Hit);
        Manager.Audio.PlayEffect("TakeDamage");
        photonView.RPC("TriggerHitAnimation",RpcTarget.AllViaServer);
        yield return new WaitForSeconds(hitRecoveryTimer);
        StopCoroutine(hitRoutine);
        hitRoutine = null;
    }

    [PunRPC]
    protected void TriggerHitAnimation() // 코루틴 HitRoutine에 의해서 호출됨. 로컬만 호출하기 때문에 RPC로 처리.
    {
        humanAnimator.SetTrigger(HitTrigger);
    }
    
}

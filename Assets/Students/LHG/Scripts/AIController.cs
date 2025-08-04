using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AIController : BaseController
{
    public MoveModule MoveModule { get; private set; }
    public FireModule FireModule { get; private set; }
    public DetectModule DetectModule { get; private set; }
    public AIStateMachine StateMachine { get; private set; }

    [Header("Set Values")]
    // [SerializeField] public float moveSpeed = 5; 상위로 올림
    [SerializeField] public float fireInterval = 1; //발사간격(초)
    [SerializeField] public float detectInterval = 1f; //탐지간격(초)
    [SerializeField] public float detectRadius = 10f; //탐지 범위(m)
    
    
    [Header("Set Spawn")]
    public Transform spawnPoint;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;


    [HideInInspector]
    public bool isFiring;
    
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
    
    private void Update()
    {
        if (photonView.IsMine)
        {
            StateMachine.Update();
            TestTeamSelection(); // TODO: 테스트코드 삭제할 것.
        }
    }

    void Start()
    {
        // 맨 처음 게임 시작 시 원격은 Ink발사가 안되는 문제 해결
        if (!photonView.IsMine)
        {
            inkParticleGun.FireParticle(MyTeam,true);
        }
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
    }

    private void MineAnimationProcess()
    {
        if (humanAnimator != null)
        {
            // Move 관련 입력 처리
            humanAnimator.SetBool(IsMove,IsMoving);
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
            humanAnimator.SetBool(Fire,isFiring);
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
                humanAnimator.SetBool(IsMove,IsMoving);
                
                // Move 관련 입력 처리
                humanAnimator.SetFloat(MoveX, networkMoveX);
                humanAnimator.SetFloat(MoveY, networkMoveY);
                
                // 공격 중이면 공격 모션
                humanAnimator.SetBool(Fire,isFiring);
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
        // TODO: 리스폰 포인트로 이동
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        
        humanModel.SetActive(true);
        col.enabled = true;
        
        
        //AI 리셋
        CurHp = MaxHp;
        IsDead = false;
        StateMachine.SetState(new IdleState(this));
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
        if(stream.IsWriting)
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
        else if(stream.IsReading)
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
            isFiring = (bool)stream.ReceiveNext();
            
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
}

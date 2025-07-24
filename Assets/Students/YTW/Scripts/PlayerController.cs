using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public StateMachine stateMachine { get; private set; }
    public Dictionary<HighState, PlayerState> highStateDic { get; private set; }

    // public Player_Human HumanState { get; private set; }
    // public Player_Squid SquidState { get; private set; }   

    public Animator humanAnimator;
    public Animator squidAnimator;

    public Rigidbody rig;
    public PlayerInput input;
    public CapsuleCollider col;
    public ThirdPersonCamera tpsCamera;
    public Camera mainCamera;

    [Header("모델")]
    public GameObject humanModel;
    public GameObject squidModel;

    [Header("플레이어 설정")]
    public float turnSensitivity = 200f;
    public LayerMask groundLayer;
    public float moveSpeed = 5f;
    public float humanJumpForce = 10f;
    public float squidJumpForce = 15f;
    public float squidSpeed = 8f;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private bool isSquidNetworked = false;

    private float networkMoveX = 0f;
    private float networkMoveZ = 0f;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        col = GetComponent<CapsuleCollider>();

        humanAnimator = humanModel.GetComponentInChildren<Animator>();
        squidAnimator = squidModel.GetComponentInChildren<Animator>();

        if (photonView.IsMine)
        {
            mainCamera = Camera.main;
            tpsCamera = Camera.main.GetComponentInParent<ThirdPersonCamera>();
            tpsCamera.followTransform = this.transform;
            Init();
        }
        else
        {
            rig.isKinematic = true;
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
        if (photonView.IsMine)
        {
            if (stateMachine != null)
            {
                stateMachine.Update();
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

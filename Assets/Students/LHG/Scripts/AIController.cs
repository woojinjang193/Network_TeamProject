using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AIController : MonoBehaviour
{
    public MoveModule MoveModule { get; private set; }
    public FireModule FireModule { get; private set; }
    public DetectModule DetectModule { get; private set; }
    public AIStateMachine StateMachine { get; private set; }

    [SerializeField] public float moveSpeed = 5;
    [SerializeField] public float fireInterval = 1; //발사간격(초)
    [SerializeField] public float detectInterval = 3; //탐지간격(초)
    [SerializeField] public float health = 100;

    public Transform spawnPoint;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    
    [SerializeField] Button testDamageBtn;

    [Header("모델 설정")]
    public SkinnedMeshRenderer AIRenderer;

    [Header("팀 설정")]
    private TeamColorInfo teamColorInfo;
    public Team myTeam { get; private set; } = Team.None;


    private void Awake()
    {
        testDamageBtn.onClick.AddListener(TakeDamage); //테스트코드 삭제할 것.
        //statedic대신 모듈화한 개별 스크립트로 관리
        MoveModule = new MoveModule(this);
        FireModule = new FireModule(this);
        DetectModule = new DetectModule(this);

        StateMachine = new AIStateMachine();
        //시작시 idle상태로
        StateMachine.SetState(new IdleState(this));

        teamColorInfo = FindObjectOfType<TeamColorInfo>();
    }

    private void Update()
    {
        StateMachine.Update();
        Die();
        UpdateAIColor();
        TestTeamSelection(); //테스트코드 삭제할 것.
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }

    public void TakeDamage()
    {
        health -= 50;
    }

    public void Respawn()
    {
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        gameObject.SetActive(true);
        gameObject.GetComponent<Collider>().enabled = true;
        
        
        //AI 리셋
        health = 100;
        StateMachine.SetState(new IdleState(this));
    }

    public void Die()
    {
        if (health <= 0)
        {
            //TODO hit애니메이션 + 딜레이
            StateMachine.SetState(new DeathState(this));
            
            Debug.Log("AI 사망");
        }
    }


    private void UpdateAIColor()
    {
        if (teamColorInfo != null)
        {
            Color teamColor = teamColorInfo.GetTeamColor(myTeam);

            if (AIRenderer != null)
            {
                AIRenderer.material.color = teamColor;
            }

            if (AIRenderer != null)
            {
                AIRenderer.material.color = teamColor;
            }
        }
    }

    private void TestTeamSelection()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            myTeam = Team.Team1;
            Debug.Log($"팀 변경: {myTeam}");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            myTeam = Team.Team2;
            Debug.Log($"팀 변경: {myTeam}");
        }
    }
}

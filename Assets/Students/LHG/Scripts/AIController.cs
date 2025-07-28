using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
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

    
    [SerializeField] Button testDamageBtn;

    private void Awake()
    {
        testDamageBtn.onClick.AddListener(TakeDamage);

        //statedic대신 모듈화한 개별 스크립트로 관리
        MoveModule = new MoveModule(this);
        FireModule = new FireModule(this);
        DetectModule = new DetectModule(this);


        StateMachine = new AIStateMachine();
        //시작시 idle상태로
        StateMachine.SetState(new IdleState(this));
    }

    private void Update()
    {
        StateMachine.Update();

        if (health <= 0)
        {
            //TODO hit애니메이션 + 딜레이
            StateMachine.SetState(new DeathState(this));
            gameObject.SetActive(false);
        }
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
}

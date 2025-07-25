using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public MoveModule MoveModule {  get; private set; }
    public FireModule FireModule {  get; private set; }
    public DetectModule DetectModule {  get; private set; }
    public AIStateMachine StateMachine {  get; private set; }

    private void Awake()
    {
        //statedic대신 모듈로 개별 스크립트로 관리
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
    }
}

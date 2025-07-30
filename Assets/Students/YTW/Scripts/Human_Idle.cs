using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Idle : PlayerState
{
    private static readonly int IsMove = Animator.StringToHash("IsMove");
    private Player_Human humanState; 

    public Human_Idle(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
    }

    public override void Enter()
    {
        Debug.Log("Human_Idle 상태");
        HasPhysics = true; 

        player.humanAnimator.SetBool(IsMove, false);
    }

    public override void Update()
    {
        if (!IsGrounded())
        {
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Jump]);
            return;
        }

        if (IsGrounded() && player.input.IsJumpPressed)
        {
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Jump]);
            return;
        }

        if (player.input.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Move]);
        }
    }

    
    public override void FixedUpdate() 
    {
        float verticalVelocity = player.rig.velocity.y;

        Vector3 horizontalVelocity = new Vector3(player.rig.velocity.x, 0f, player.rig.velocity.z);

        horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.fixedDeltaTime * 30f);

        player.rig.velocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
    }

    public override void Exit() { }
}

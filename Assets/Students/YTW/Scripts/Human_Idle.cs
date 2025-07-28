using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Idle : PlayerState
{
    private Player_Human humanState; 

    public Human_Idle(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
    }

    public override void Enter()
    {
        HasPhysics = false; 
        // player.humanAnimator.SetBool("isMoving", false);
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

    public override void Exit() { }
    public override void FixedUpdate() { }
}

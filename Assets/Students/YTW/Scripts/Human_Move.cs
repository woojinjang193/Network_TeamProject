using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Move : PlayerState
{
    private static readonly int Y = Animator.StringToHash("MoveY");
    private static readonly int X = Animator.StringToHash("MoveX");
    private static readonly int IsMove = Animator.StringToHash("IsMove");
    private Player_Human humanState;

    public Human_Move(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
        HasPhysics = true; 
    }

    public override void Enter()
    {
        Debug.Log("Human_Move 상태");
        player.humanAnimator.SetBool(IsMove, true);
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

        if (player.input.MoveInput == Vector2.zero)
        {
            player.rig.velocity = new Vector3(0, player.rig.velocity.y, 0);
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Idle]);
        }
    }

    public override void FixedUpdate()
    {
        float currentSpeed = player.moveSpeed;
        if (player.CurrentGroundInkStatus == InkStatus.ENEMY_TEAM)
        {
            currentSpeed *= player.enemyInkSpeedModifier;
        }

        if (IsGrounded())
        {
            SetMove(currentSpeed);
            SetPlayerRotation();
        }

        UpdateAnimationParameters();
    }
    
    public override void Exit()
    {
        player.humanAnimator.SetBool(IsMove, false);

        if (player.humanAnimator != null)
        {
            player.humanAnimator.SetFloat(X, 0f);
            player.humanAnimator.SetFloat(Y, 0f);
        }
    }
}

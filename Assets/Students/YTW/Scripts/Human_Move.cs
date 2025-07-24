using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Move : PlayerState
{
    private Player_Human humanState;

    public Human_Move(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
        HasPhysics = true; 
    }

    public override void Enter()
    {
        Debug.Log("Human Move Enter");
        // player.humanAnimator.SetBool("isMoving", true);
    }

    public override void Update()
    {
        if (IsGrounded() && player.input.IsJumpPressed)
        {
            // if(player.humanAnimator != null) player.humanAnimator.SetTrigger("Jump");
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Jump]);
            return; 
        }

        if (player.input.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(humanState.lowStateDic[LowState.Idle]);
        }
    }

    public override void FixedUpdate()
    {
        SetMove(player.moveSpeed);
        SetPlayerRotation();
        UpdateAnimationParameters();
    }
    private void UpdateAnimationParameters()
    {
        if (player.humanAnimator == null) return;

        Vector3 worldMoveDirection = player.rig.velocity;
        worldMoveDirection.y = 0;

        Vector3 localMoveDirection = player.transform.InverseTransformDirection(worldMoveDirection.normalized);

        player.humanAnimator.SetFloat("moveX", localMoveDirection.x, 0.1f, Time.fixedDeltaTime);
        player.humanAnimator.SetFloat("moveZ", localMoveDirection.z, 0.1f, Time.fixedDeltaTime);
    }
    public override void Exit()
    {
        // player.humanAnimator.SetBool("isMoving", false);

        if (player.humanAnimator != null)
        {
            player.humanAnimator.SetFloat("moveX", 0f);
            player.humanAnimator.SetFloat("moveZ", 0f);
        }
    }
}

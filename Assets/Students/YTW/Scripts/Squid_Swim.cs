using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squid_Swim : PlayerState
{
    private Player_Squid squidState;
    public Squid_Swim(PlayerController player, StateMachine stateMachine, Player_Squid squidState) : base(player, stateMachine)
    { 
        this.squidState = squidState; 
        HasPhysics = true; 
    }

    public override void Update()
    {
        if (player.input.IsJumpPressed && IsGrounded())
        {
            player.rig.useGravity = true; 
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (!IsGrounded())
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (player.input.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Idle]);
        }
    }
    public override void FixedUpdate()
    {
        if (player.IsOnWalkableWall && player.input.MoveInput.y > 0.1f)
        {
            ClimbWall();
        }
        else
        {
            SwimOnGround();
        }
    }

    private void ClimbWall()
    {
        Vector3 wallClimbVelocity = Vector3.up * player.squidSpeed;
        player.rig.velocity = wallClimbVelocity;
    }

    private void SwimOnGround()
    {
        // 잉크 속에 숨어서 움직임 표현
        if (player.rig.velocity.y > 0.1f) return;

        Vector3 moveDirection = new Vector3(player.input.MoveInput.x, 0f, player.input.MoveInput.y);
        Vector3 cameraForward = player.mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        Vector3 cameraRight = player.mainCamera.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 moveVec = (cameraForward * moveDirection.z + cameraRight * moveDirection.x).normalized;

        player.rig.velocity = new Vector3(moveVec.x * player.squidSpeed, -1.0f, moveVec.z * player.squidSpeed);
        SetPlayerRotation();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController player;
    protected StateMachine stateMachine;
    

    public PlayerState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }


    protected void SetMove(float moveSpeed)
    {
        Vector3 camForward = player.mainCamera.transform.forward;
        Vector3 camRight = player.mainCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 moveDirection = (camForward.normalized * player.input.MoveInput.y +
                                 camRight.normalized * player.input.MoveInput.x);

        // 경사면 이동일 경우
        if (player.IsGrounded)
        {
            Vector3 projectedMove = Vector3.ProjectOnPlane(moveDirection, player.GroundNormal).normalized * moveDirection.magnitude;
            player.rig.velocity = new Vector3(projectedMove.x * moveSpeed, player.rig.velocity.y, projectedMove.z * moveSpeed);
        }
        else 
        {
           player.rig.velocity = new Vector3(moveDirection.x * moveSpeed, player.rig.velocity.y, moveDirection.z * moveSpeed);
        }
    }
    protected void SetPlayerRotation()
    {
        Vector3 lookDirection = player.rig.velocity;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }
    }

    protected void Jump(float jumpForce)
    {
        player.rig.useGravity = true;
        player.rig.velocity = new Vector3(player.rig.velocity.x, jumpForce, player.rig.velocity.z);
    }
    protected bool IsGrounded()
    {
        return player.IsGrounded;
    }

    protected void Die()
    {
        /* 사망 로직 구현 */
    }

    public override void Enter()
    {

    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {

    }
}

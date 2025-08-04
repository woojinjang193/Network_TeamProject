using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    protected PlayerController player;
    protected StateMachine stateMachine;


    protected PlayerState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }


    protected void SetMove(float moveSpeed)
    {
        if (player.input.MoveInput == Vector2.zero)
        {
            player.rig.velocity = new Vector3(0, player.rig.velocity.y, 0);
            player.humanAnimator.SetFloat(MoveX,0f);
            player.humanAnimator.SetFloat(MoveY,0f);
            return;
        }
        
        Vector3 camForward = player.mainCamera.transform.forward;
        Vector3 camRight = player.mainCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 moveDirection = (camForward * player.input.MoveInput.y +
                                 camRight * player.input.MoveInput.x).normalized;

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
        if (player.IsFiring || player.ModelTransform == null)
        {
            return;
        }

        Vector3 lookDirection = player.rig.velocity;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            player.ModelTransform.rotation = Quaternion.Slerp(player.ModelTransform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }
    }

    protected void UpdateAnimationParameters()
    {
        if (player.humanAnimator == null) return;

        Vector3 worldMoveDirection = player.rig.velocity;
        if (player.rig.velocity == Vector3.zero)
        {
            player.humanAnimator.SetFloat(MoveX, 0f);
            player.humanAnimator.SetFloat(MoveY, 0f);
            return;
        }
        worldMoveDirection.y = 0;

        Vector3 localMoveDirection = player.ModelTransform.InverseTransformDirection(worldMoveDirection.normalized);

        player.humanAnimator.SetFloat(MoveX, localMoveDirection.x, 0.1f, Time.fixedDeltaTime);
        player.humanAnimator.SetFloat(MoveY, localMoveDirection.z, 0.1f, Time.fixedDeltaTime);
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

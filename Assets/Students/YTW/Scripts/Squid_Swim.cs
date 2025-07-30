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
            Jump(player.squidJumpForce);
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (!IsGrounded() && !player.IsOnWalkableWall)
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
        if (player.IsAtWallEdge && player.input.MoveInput.y > 0.1f)
        {
            VaultOverWall();
        }
        else if (player.IsOnWalkableWall)
        {
            MoveOnWall();
        }
        else
        {
            SwimOnGround();
        }
    }

    private void MoveOnWall()
    {
        player.IsVaulting = false;
        Quaternion cameraYaw = Quaternion.Euler(0, player.mainCamera.transform.eulerAngles.y, 0);
        Vector3 moveDirection = new Vector3(player.input.MoveInput.x, player.input.MoveInput.y, 0);
        Vector3 targetDirection = cameraYaw * moveDirection;
        player.rig.velocity = targetDirection.normalized * player.squidSpeed;
        Quaternion targetRotation = Quaternion.LookRotation(-player.WallNormal);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
    }

    private void VaultOverWall()
    {
        player.IsVaulting = true;
        player.rig.velocity = Vector3.zero;

        Vector3 upwardForce = Vector3.up * player.squidJumpForce * 1.2f;
        Vector3 forwardForce = player.transform.forward * player.squidSpeed * 0.5f;

        player.rig.AddForce(upwardForce + forwardForce, ForceMode.Impulse);
    }

    private void SwimOnGround()
    {
        player.IsVaulting = false;
        SetMove(player.squidSpeed);
        SetPlayerRotation();
    }
}

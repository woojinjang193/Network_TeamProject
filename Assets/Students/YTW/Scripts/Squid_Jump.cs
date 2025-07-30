using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squid_Jump : PlayerState
{
    private Player_Squid squidState;

    public Squid_Jump(PlayerController player, StateMachine stateMachine, Player_Squid squidState) : base(player, stateMachine)
    {
        this.squidState = squidState;
        HasPhysics = true;
    }

    public override void Enter()
    {
        player.IsVaulting = false;

        if (player.input.IsJumpPressed)
        {
            Jump(player.squidJumpForce);
        }
        else
        {
            player.rig.useGravity = true;
        }
    }

    public override void FixedUpdate()
    {
        if (player.rig.velocity.y < 0.1f && IsGrounded())
        {
            player.IsVaulting = false;
            if (player.input.MoveInput != Vector2.zero)
            {
                stateMachine.ChangeState(squidState.lowStateDic[LowState.Move]);
            }
            else
            {
                stateMachine.ChangeState(squidState.lowStateDic[LowState.Idle]);
            }
            return;
        }

        if (!player.IsVaulting)
        {
            SetMove(player.moveSpeed * 0.9f); 
            SetPlayerRotation(); 
        }
    }
}

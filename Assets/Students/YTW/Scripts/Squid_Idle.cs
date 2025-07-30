using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squid_Idle : PlayerState
{
    private Player_Squid squidState;
    public Squid_Idle(PlayerController player, StateMachine stateMachine, Player_Squid squidState) : base(player, stateMachine)
    {
        this.squidState = squidState; 
        HasPhysics = true; 
    }

    public override void Enter()
    {
        
    }
    public override void Update()
    {
        if (player.input.IsJumpPressed && IsGrounded())
        {
            player.rig.useGravity = true; // 점프할 때는 중력 활성화
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (!IsGrounded())
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (player.input.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Move]);
        }
    }
    public override void FixedUpdate()
    {
        if (player.rig.velocity.y > 0.1f)
        {
            return;
        }
        player.rig.velocity = new Vector3(0f, -1.0f, 0f);
    }
}

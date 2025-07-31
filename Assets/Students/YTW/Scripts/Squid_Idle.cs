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
            Jump(player.squidJumpForce);
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
        float verticalVelocity = player.rig.velocity.y;
        Vector3 horizontalVelocity = new Vector3(player.rig.velocity.x, 0f, player.rig.velocity.z);

        horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.fixedDeltaTime * 20f); 

        player.rig.velocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
    }
}

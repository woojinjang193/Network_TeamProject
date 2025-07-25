using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squid_Swim : PlayerState
{
    private Player_Squid squidState;
    public Squid_Swim(PlayerController player, StateMachine stateMachine, Player_Squid squidState) : base(player, stateMachine)
    { this.squidState = squidState; HasPhysics = true; }

    public override void Update()
    {
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
        SetMove(player.squidSpeed);
        SetPlayerRotation();
    }
}

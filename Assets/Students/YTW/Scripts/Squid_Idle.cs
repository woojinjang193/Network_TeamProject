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

    public override void Update()
    {
        if (player.input.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Move]);
        }
    }
    public override void FixedUpdate()
    {
        player.rig.velocity = Vector3.zero; 
    }
}

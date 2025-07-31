using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Jump : PlayerState
{
    private Player_Human humanState;
    public Human_Jump(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
        HasPhysics = true;
    }

    public override void Enter()
    {
        Debug.Log("Human_Jump 상태");
        if (player.input.IsJumpPressed)
        {
            Jump(player.humanJumpForce);
        }
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {
        SetMove(player.moveSpeed);
        SetPlayerRotation();

        if (player.rig.velocity.y < 0.1f && IsGrounded())
        {
            if (player.input.MoveInput != Vector2.zero)
            {
                stateMachine.ChangeState(humanState.lowStateDic[LowState.Move]);
            }
            else
            {
                stateMachine.ChangeState(humanState.lowStateDic[LowState.Idle]);
            }
        }
    }

    public override void Exit() { }
}

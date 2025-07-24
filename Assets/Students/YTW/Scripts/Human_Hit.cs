using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human_Hit : PlayerState
{
    private Player_Human humanState;

    public Human_Hit(PlayerController player, StateMachine stateMachine, Player_Human humanState) : base(player, stateMachine)
    {
        this.humanState = humanState;
        HasPhysics = true;
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

    public override void Exit() { }
}

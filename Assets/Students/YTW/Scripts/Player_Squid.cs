using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Squid : PlayerState
{
    private StateMachine subStateMachine;
    public Dictionary<LowState, BaseState> lowStateDic { get; private set; }

    public Player_Squid(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
        HasPhysics = true;
        subStateMachine = new StateMachine();
        lowStateDic = new Dictionary<LowState, BaseState>();

        lowStateDic.Add(LowState.Idle, new Squid_Idle(player, subStateMachine, this));
        lowStateDic.Add(LowState.Move, new Squid_Swim(player, subStateMachine, this));
    }

    public override void Enter()
    {
        Debug.Log("오징어 폼");
        player.gameObject.layer = LayerMask.NameToLayer("Invincible");
        player.humanModel.SetActive(false);
        player.squidModel.SetActive(true);
        player.col.height = 1.0f;
        player.col.radius = 0.5f;
        player.rig.useGravity = false;
        player.rig.velocity = Vector3.zero;

        subStateMachine.Initialize(lowStateDic[LowState.Idle]);
    }

    public override void Update()
    {
        subStateMachine.Update();

        if (!player.input.IsSquidHeld)
        {
            this.stateMachine.ChangeState(player.highStateDic[HighState.HumanForm]);
        }
    }

    public override void FixedUpdate()
    {
        subStateMachine.FixedUpdate();
    }

    public override void Exit() 
    {
        player.rig.useGravity = true;
    }
}

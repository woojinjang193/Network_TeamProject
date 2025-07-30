using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Squid : PlayerState
{
    private StateMachine subStateMachine;
    public Dictionary<LowState, BaseState> lowStateDic { get; private set; }

    private float revertTimer = 0f;
    private const float REVERT_DELAY = 0.1f;
    private Vector3 colCenter = Vector3.zero;

    public Player_Squid(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
        HasPhysics = true;
        subStateMachine = new StateMachine();
        lowStateDic = new Dictionary<LowState, BaseState>();

        lowStateDic.Add(LowState.Idle, new Squid_Idle(player, subStateMachine, this));
        lowStateDic.Add(LowState.Move, new Squid_Swim(player, subStateMachine, this));
        lowStateDic.Add(LowState.Jump, new Squid_Jump(player, subStateMachine, this));
    }

    public override void Enter()
    {
        Debug.Log("오징어 폼");
        player.gameObject.layer = LayerMask.NameToLayer("Invincible");
        player.humanModel.SetActive(false);
        player.squidModel.SetActive(true);
        player.col.direction = 2;
        player.col.height = 1.0f;
        player.col.radius = 0.2f;
        player.col.center = colCenter;

        player.rig.useGravity = false;
        player.rig.velocity = Vector3.zero;

        revertTimer = 0f;
        subStateMachine.Initialize(lowStateDic[LowState.Idle]);
    }

    public override void Update()
    {
        subStateMachine.Update();

        if (!player.input.IsSquidHeld)
        {
            this.stateMachine.ChangeState(player.highStateDic[HighState.HumanForm]);
            return;
        }


        if (player.IsGrounded && player.CurrentGroundInkStatus != InkStatus.OUR_TEAM)
        {
            revertTimer += Time.deltaTime;
            if (revertTimer >= REVERT_DELAY)
            {
                this.stateMachine.ChangeState(player.highStateDic[HighState.HumanForm]);
            }
        }
        else
        {
            revertTimer = 0f;
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

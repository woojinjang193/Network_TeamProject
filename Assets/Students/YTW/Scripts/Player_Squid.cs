using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Squid : PlayerState
{
    private StateMachine subStateMachine;
    public Dictionary<LowState, BaseState> lowStateDic { get; private set; }

    private float revertTimer = 0f;
    private const float REVERT_DELAY = 0.1f;

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
        player.col.height = 1.0f;
        player.col.radius = 0.5f;
        player.IsVaulting = false;
        player.rig.useGravity = false;
        player.rig.velocity = Vector3.zero;

        revertTimer = 0f;
        subStateMachine.Initialize(lowStateDic[LowState.Idle]);
    }

    public override void Update()
    {
        subStateMachine.Update();

        if (!player.input.IsSquidHeld && !player.IsOnWalkableWall)
        {
            this.stateMachine.ChangeState(player.highStateDic[HighState.HumanForm]);
            return;
        }

        if (player.WallNormal != Vector3.zero && !player.IsGrounded && player.CurrentWallInkStatus != InkStatus.OUR_TEAM)
        {
            player.rig.AddForce(player.WallNormal * 5f, ForceMode.Impulse);

            this.stateMachine.ChangeState(player.highStateDic[HighState.HumanForm]);
            return;
        }

        // 3. 땅에 있지만, 우리 팀 잉크가 아닐 경우 (기존 로직)
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

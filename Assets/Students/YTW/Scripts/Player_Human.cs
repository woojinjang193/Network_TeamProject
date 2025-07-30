using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Human : PlayerState
{
    private StateMachine subStateMachine;
    public Dictionary<LowState, BaseState> lowStateDic { get; private set; }
    private Vector3 colCenter = new Vector3(0, 0.5f, 0);

    public Player_Human(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
        HasPhysics = true;
        subStateMachine = new StateMachine();
        lowStateDic = new Dictionary<LowState, BaseState>();

        lowStateDic.Add(LowState.Idle, new Human_Idle(player, subStateMachine, this));
        lowStateDic.Add(LowState.Move, new Human_Move(player, subStateMachine, this));
        lowStateDic.Add(LowState.Jump, new Human_Jump(player, subStateMachine, this));
        lowStateDic.Add(LowState.Hit, new Human_Hit(player, subStateMachine, this));
    }

    public override void Enter()
    {
        Debug.Log("인간 폼");
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        player.humanModel.SetActive(true);
        player.squidModel.SetActive(false);
        player.col.direction = 1;
        player.col.center = colCenter;
        player.col.height = 1.0f;
        player.col.radius = 0.25f;
        subStateMachine.Initialize(lowStateDic[LowState.Idle]);
    }

    public override void Update()
    {
        subStateMachine.Update();

        HandleShooting();

        if (player.input.IsSquidHeld)
        {
            if (player.IsGrounded && player.CurrentGroundInkStatus == InkStatus.OUR_TEAM)
            {
                this.stateMachine.ChangeState(player.highStateDic[HighState.SquidForm]);
            }
            else if (player.input.IsSquidHeld)
            {
                Debug.Log("<color=orange>오징어 변신 실패: 우리 팀 잉크 위가 아닙니다.</color>");
            }
        }

    }

    private void HandleShooting()
    {
        if (player.weaponView != null)
        {
            if (player.input.IsFirePressed)
            {
                player.weaponView.RPC("FireParticle", RpcTarget.All, player.MyTeam, true);
            }
            if (player.input.IsFireReleased)
            {
                player.weaponView.RPC("FireParticle", RpcTarget.All, player.MyTeam, false);
            }
        }
        else
        {
            if (player.input.IsFirePressed)
            {
                Debug.LogError("weaponView가 null입니다");
            }
        }
    }

    public override void FixedUpdate()
    {
        subStateMachine.FixedUpdate();
    }

    public override void Exit()
    {
        if (player.weaponView != null)
        {
            player.weaponView.RPC("FireParticle", RpcTarget.All, player.MyTeam, false);
        }
    }

}

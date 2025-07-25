using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Human : PlayerState
{
    private StateMachine subStateMachine;
    public Dictionary<LowState, BaseState> lowStateDic { get; private set; }

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
        player.col.height = 2.0f;
        player.col.radius = 0.5f;
        subStateMachine.Initialize(lowStateDic[LowState.Idle]);
    }

    public override void Update()
    {
        subStateMachine.Update();

        HandleShooting();

        if (IsGrounded() && player.input.IsSquidHeld)
        {
            this.stateMachine.ChangeState(player.highStateDic[HighState.SquidForm]);
        }

    }

    private void HandleShooting()
    {
        if (player.weaponView != null)
        {
            if (player.input.IsFirePressed)
            {
                player.weaponView.RPC("FireParticle", RpcTarget.All, player.myTeam, true);
            }
            if (player.input.IsFireReleased)
            {
                player.weaponView.RPC("FireParticle", RpcTarget.All, player.myTeam, false);
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

        if (!IsGrounded())
        {
            if (player.rig.velocity.y >= 0)
            {
                player.rig.velocity += Vector3.up * Physics.gravity.y * (player.gravityScale - 1) * Time.fixedDeltaTime;
            }
            else
            {
                player.rig.velocity += Vector3.up * Physics.gravity.y * (player.fallingGravityScale - 1) * Time.fixedDeltaTime;
            }
        }
    }

    public override void Exit()
    {
        if (player.weaponView != null)
        {
            player.weaponView.RPC("FireParticle", RpcTarget.All, player.myTeam, false);
        }
    }

}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Squid_Swim : PlayerState
{
    private static readonly int IsMove = Animator.StringToHash("IsMove");
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private Player_Squid squidState;
    private event UnityAction OnSwimStart ;
    private event UnityAction OnSwimStop ;
    private bool isSwiming;
    public Squid_Swim(PlayerController player, StateMachine stateMachine, Player_Squid squidState) : base(player, stateMachine)
    { 
        this.squidState = squidState; 
        HasPhysics = true; 
    }

    public override void Enter()
    {
        Debug.Log("squid Swim상태");
        player.squidAnimator.SetBool(IsMove,true);
        SetPlaySound();
    }
    public override void Update()
    {
        if (player.input.IsJumpPressed && IsGrounded())
        {
            Jump(player.squidJumpForce);
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (!IsGrounded() && !player.IsOnWalkableWall)
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Jump]);
            return;
        }

        if (player.input.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeState(squidState.lowStateDic[LowState.Idle]);
        }
    }
    public override void FixedUpdate()
    {
        if (player.IsAtWallEdge && player.input.MoveInput.y > 0.1f)
        {
            VaultOverWall();
            if (isSwiming)
            {
                isSwiming = false;
            }
        }
        else if (player.IsOnWalkableWall)
        {
            MoveOnWall();
        }
        else
        {
            SwimOnGround();
            if (isSwiming)
            {
                isSwiming = false;
            }
        }

        if (OnSwimStart != null)
        {
            OnSwimStart.Invoke();
            OnSwimStart -= SwimStarted;
            OnSwimStop += SwimEnded;
        }

        if (OnSwimStop != null && !isSwiming)
        {
            OnSwimStop.Invoke();
            OnSwimStop -= SwimEnded;
        }
        SetAnimatorParameter(); 
    }

    public override void Exit()
    {
        player.squidAnimator.SetBool(IsMove, false);
        if (OnSwimStop != null)
        {
            OnSwimStop.Invoke();
            OnSwimStop = null;
        }

        StopSound();
    }
    
    private void MoveOnWall()
    {
        if (!isSwiming)
        {
            isSwiming = true;
            OnSwimStart += SwimStarted;
        }
        player.IsVaulting = false;
        Quaternion cameraYaw = Quaternion.Euler(0, player.mainCamera.transform.eulerAngles.y, 0);
        Vector3 moveDirection = new Vector3(player.input.MoveInput.x, player.input.MoveInput.y, 0);
        Vector3 targetDirection = cameraYaw * moveDirection;
        player.rig.velocity = targetDirection.normalized * player.squidSpeed;
        Quaternion targetRotation = Quaternion.LookRotation(-player.WallNormal);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
    }

    private void VaultOverWall()
    {
        player.IsVaulting = true;
        player.rig.velocity = Vector3.zero;

        Vector3 upwardForce = Vector3.up * player.squidJumpForce * 1.2f;
        Vector3 forwardForce = player.transform.forward * player.squidSpeed * 0.5f;

        player.rig.AddForce(upwardForce + forwardForce, ForceMode.Impulse);
    }

    private void SwimOnGround()
    {
        player.IsVaulting = false;
        SetMove(player.squidSpeed);
        SetPlayerRotation();
    }

    private void SetAnimatorParameter()
    {
        player.squidAnimator.SetFloat(MoveSpeed, player.rig.velocity.magnitude/player.squidSpeed);
    }

    private void SwimStarted()
    {
        player.squidModel.transform.Rotate(-90,0,0);
    }

    private void SwimEnded()
    {
        player.squidModel.transform.Rotate(90,0,0);
    }

    private void SetPlaySound()
    {
        if (!player.squidSwim)
        {
            player.squidSwim = Manager.Audio.PlayClip("Swim", player.transform.position);
            player.squidSwim.transform.SetParent(player.transform);
        }
        if (!player.squidSwimBubble)
        {
            player.squidSwimBubble = Manager.Audio.PlayClip("Bubble", player.transform.position);
            player.squidSwimBubble.transform.SetParent(player.transform);
        }
        player.squidSwim.Play();
        player.squidSwimBubble.Play();
    }

    private void StopSound()
    {
        if (player.squidSwim.isPlaying)
        {
            player.squidSwim.Stop();
        }

        if (player.squidSwimBubble.isPlaying)
        {
            player.squidSwimBubble.Stop();
        }
    }
}

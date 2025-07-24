using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController player;
    protected StateMachine stateMachine;
    

    public PlayerState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }


    protected void SetMove(float moveSpeed)
    {
        Vector3 camForward = player.mainCamera.transform.forward;
        Vector3 camRight = player.mainCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 moveDirection = (camForward.normalized * player.input.MoveInput.y +
                                 camRight.normalized * player.input.MoveInput.x);

        player.rig.velocity = new Vector3(moveDirection.x * moveSpeed, player.rig.velocity.y, moveDirection.z * moveSpeed);
    }
    protected void SetPlayerRotation()
    {
        Vector3 lookDirection = player.rig.velocity;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            player.rig.rotation = Quaternion.Slerp(player.rig.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }

  
    protected void Jump(float jumpForce)
    {
        player.rig.velocity = new Vector3(player.rig.velocity.x, 0, player.rig.velocity.z);
        player.rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    protected bool IsGrounded()
    {
        float rayDistance = 1.1f;
        bool isGrounded = Physics.Raycast(player.transform.position, Vector3.down, rayDistance, player.groundLayer);

        Debug.DrawRay(player.transform.position, Vector3.down * rayDistance, Color.red);

        return isGrounded;
    }

    protected void Die()
    {
        /* 사망 로직 구현 */
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

    public override void Exit()
    {

    }
}

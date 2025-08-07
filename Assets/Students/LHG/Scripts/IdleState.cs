using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 초기상태. Wander, 탐지, (조건충족시)발사를 수행
/// </summary>
public class IdleState : AIBaseState
{
    public IdleState(AIController controller) : base(controller) { }

    public override void OnEnter()
    {
        _controller.DetectModule.detectTimer = 0f;
    }
    public override void OnUpdate()
    {
        if (!_controller.canControl) return;//////////////

        _controller.MoveModule.StartPatrol(); //**여기**
        _controller.DetectModule.Update();
        _controller.FireModule.TryFireAt(_controller.DetectModule.Target);

        if(_controller.DetectModule.HasEnemy)
        {
            _controller.MoveModule.StopPatrol();
            _controller.StateMachine.SetState(new ChaseState(_controller));
        }
    }

    public override void OnExit()
    {
        _controller.MoveModule.StopPatrol();
    }
}

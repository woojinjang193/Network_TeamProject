using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 초기상태. Wander, 탐지, (조건충족시)발사를 수행
/// </summary>
public class IdleState : AIBaseState
{
    public IdleState(AIController controller) : base(controller) { }

    public override void OnUpdate()
    {
        _controller.MoveModule.Wander();
        _controller.DetectModule.Update();

        if(_controller.DetectModule.HasEnemy)
        {
            _controller.FireModule.FireAt(_controller.DetectModule.Target);
            _controller.MoveModule.StopWander();
            _controller.StateMachine.SetState(new ChaseState(_controller));
        }
        
        //if (_controller.DetectModule.HasEnemy)
        //{
        //    _controller.StateMachine.SetState(new ChaseState(_controller));
        //}
    }
}

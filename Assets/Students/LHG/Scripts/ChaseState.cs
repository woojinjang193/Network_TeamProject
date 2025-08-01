using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// DetectModule에서 감지한 타겟을 가져와서 타겟 포지션으로 이동과 발사를 수행
/// </summary>
public class ChaseState : AIBaseState
{
    public ChaseState(AIController controller) : base(controller){}

    public override void OnUpdate()
    {
        var target = _controller.DetectModule.Target;
        if(target == null)
        {
            //타겟이 없을 경우 idle상태로 전환
            _controller.StateMachine.SetState(new IdleState(_controller));
            return;
        }

        float distance = Vector3.Distance(_controller.transform.position, target.position);
        if(distance > 10f)
        {
            _controller.StateMachine.SetState(new IdleState(_controller));
            _controller.FireModule.StopFire();
            _controller.DetectModule.Target = null;
        }
        else if (distance > 3f)
        {
            _controller.FaceOff(FaceType.Upset);
            _controller.IsMoving = true;
            _controller.MoveModule.MoveTo(target.position);
        }
        else
        {
            _controller.IsMoving = false;
        }
        _controller.FireModule.TryFireAt(target);
    }
}

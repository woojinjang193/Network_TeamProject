
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveModule
{
    private AIController _controller;
    private Coroutine _patrolRoutine;
    private int _currentPatrolIndex = 0;

    private List<Transform> _patrolPoints;

    public MoveModule(AIController controller)
    {
        _controller = controller;
    }


    public void SetPatrolPoints(List<Transform> patrolPoints)
    {
        _patrolPoints = patrolPoints;
        _currentPatrolIndex = 0;
    }

    public void StartPatrol()
    {
        if (_patrolPoints == null || _patrolPoints.Count == 0) return;
        if (_patrolRoutine == null)
        {
            _controller.FaceOff(FaceType.Idle);
            _patrolRoutine = _controller.StartCoroutine(PatrolRoutine());
        }
    }

    public void StopPatrol()
    {
        if (_patrolRoutine != null)
        {
            _controller.StopCoroutine(_patrolRoutine);
            _patrolRoutine = null;
            _controller.agent.ResetPath();
        }
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (!_controller.agent.pathPending && _controller.agent.remainingDistance < _controller.agent.stoppingDistance + 0.1f)
            {
                //_controller.IsMoving = false;

                yield return new WaitForSeconds(1f); // 도착 후 대기 시간

                // 랜덤으로 다음 패트롤 포인트 선택
                int nextIndex;
                do
                {
                    nextIndex = Random.Range(0, _patrolPoints.Count);
                } while (_patrolPoints.Count > 1 && nextIndex == _currentPatrolIndex); // 같은 곳 두 번 연속 방지

                _currentPatrolIndex = nextIndex;

                //  같은 포인트라도 약간의 랜덤 오프셋 부여 
                Vector3 targetPos = _patrolPoints[_currentPatrolIndex].position +
                                    new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

                MoveTo(_patrolPoints[_currentPatrolIndex].position);
            }

            yield return null;
        }
    }


    public void MoveTo(Vector3 targetPos)
    {
        float currentSpeed = _controller.moveSpeed;

        if (_controller.CurrentGroundInkStatus == InkStatus.ENEMY_TEAM)
        {
            currentSpeed *= _controller.enemyInkSpeedModifier;
        }

        if (!_controller.agent.hasPath || _controller.agent.destination != targetPos)
        {
            _controller.agent.speed = currentSpeed;
            _controller.agent.SetDestination(targetPos);
            _controller.IsMoving = true;
        }

        else
        {
            //_controller.IsMoving = false;
        }
        //_controller.IsMoving = !_controller.agent.pathPending && _controller.agent.remainingDistance > _controller.agent.stoppingDistance;
    }

    public Vector3 GetDirection(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - _controller.transform.position).normalized;
        direction.y = 0;
        return direction;
    }

    public void RotateToTarget(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            _controller.transform.rotation = Quaternion.Slerp(
                _controller.transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );
        }
    }
}

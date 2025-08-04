using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModule
{
    private AIController _controller;
    private Vector3 _randomPos;
    private Coroutine _wanderRoutine;
    private float _wanderTime = 3f;

    public MoveModule(AIController controller)
    {
        _controller = controller;
    }

    public void Wander()
    {
        if (_wanderRoutine == null)
        {
            _controller.FaceOff(FaceType.Idle);
            _wanderRoutine = _controller.StartCoroutine(WanderRoutine());
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            float randomDistance = SetNewRandomTargetPosition();
            if (randomDistance > 7f)
            {
                float elapsed = 0f;

                while (elapsed < _wanderTime)
                {
                    MoveTo(_randomPos);
                    elapsed += Time.deltaTime;
                
                    float distance = Vector3.Distance(_controller.transform.position, _randomPos);
                    _controller.IsMoving = distance > 0.1f;
                
                    yield return null;
                }
            }
            yield return null;
        }
    }

    private float SetNewRandomTargetPosition()
    {
        Vector2 circle = Random.insideUnitCircle * Random.Range(5f, 12f);
        _randomPos = _controller.transform.position + new Vector3(circle.x, 0, circle.y);
        return Vector3.Distance(_controller.transform.position, _randomPos);
    }

    public void MoveTo(Vector3 targetPos)
    {
        Vector3 direction = GetDirection(targetPos);
        _controller.transform.position += direction * (_controller.moveSpeed * Time.deltaTime);
        
        RotateToTarget(direction);
    }

    public void StopWander()
    {
        if (_wanderRoutine != null)
        {
            _controller.StopCoroutine(_wanderRoutine);
            _wanderRoutine = null;
        }
    }

    // 벽에 부딪히면 이동 중단하고 방향 재설정
    public void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        float angle = Vector3.Dot(contact.normal, -_controller.transform.forward);

        if (angle > 0.5f) // 벽이라고 판단
        {
            Debug.Log("벽 충돌시 이동 중단 및 재설정");
            StopWander();
            Wander();
        }
    }

    public Vector3 GetDirection(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - _controller.transform.position).normalized;
        direction.y = 0;
        return direction;
    }

    public void RotateToTarget(Vector3 direction)
    {
        if (Vector3.Dot(direction, _controller.transform.forward)>0.98f)
            // 거의 각도가 맞으니 return;
        {
            return;
        }
        
        if (direction != Vector3.zero)
        {
            _controller.transform.rotation = Quaternion.Slerp(
                _controller.transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f
            );
        }
    }
}


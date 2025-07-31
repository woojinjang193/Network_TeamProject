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
            _wanderRoutine = _controller.StartCoroutine(WanderRoutine());
        }
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            SetNewRandomTargetPosition();
            float elapsed = 0f;

            while (elapsed < _wanderTime)
            {
                MoveTo(_randomPos);
                elapsed += Time.deltaTime;

                // 적 발견 시 중단
                if (_controller.DetectModule.HasEnemy)
                {
                    StopWander();
                    yield break;
                }

                yield return null;
            }

            yield return null;
        }
    }

    private void SetNewRandomTargetPosition()
    {
        Vector2 circle = Random.insideUnitCircle * Random.Range(3f, 6f);
        _randomPos = _controller.transform.position + new Vector3(circle.x, 0, circle.y);
    }

    public void MoveTo(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - _controller.transform.position).normalized;
        direction.y = 0;
        _controller.transform.position += direction * _controller.moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
            _controller.transform.rotation = Quaternion.Slerp(
                _controller.transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f
            );
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
        float angle = Vector3.Angle(contact.normal, Vector3.up);

        if (angle > 45f) // 벽이라고 판단
        {
            Debug.Log("벽 충돌시 이동 중단 및 재설정");
            StopWander();
            Wander();
        }
    }
}


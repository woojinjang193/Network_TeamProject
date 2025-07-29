using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DeathState : AIBaseState
{
    private float _deathTime = 3f;
    private float _timer;

    public DeathState(AIController controller) : base(controller) { }
    

    public override void OnEnter()
    {
        //TODO애니메이터 사망모션 +딜레이시간 필요함.. 코루틴으로 생각해보기

        _timer = 0f;
        _controller.GetComponent<Collider>().enabled = false;
        //_controller.enabled = false;
        _controller.gameObject.SetActive(false);
        Debug.Log("데쓰 온엔터");
    }

    public override void OnUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer >= _deathTime)
        {
            _controller.Respawn();
        }
        Debug.Log("데쓰 온업데이트");
    }
}

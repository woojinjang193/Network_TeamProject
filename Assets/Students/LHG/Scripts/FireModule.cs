using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireModule
{
    private AIController _controller;

    public FireModule(AIController controller)
    {
        _controller = controller;
    }

    

    public void FireAt(Transform target)
    {
        //TODO 플레이어 기능의 물감발사를 가져오자 + 발사간격
        
        Vector3 dir = (target.position - _controller.transform.position).normalized;
        Debug.DrawRay(_controller.transform.position, dir * 5f, Color.red, 0.2f);
        //Debug.Log("타겟에게 발사, 타겟 위치:" + target.position);
        Debug.Log("발사 방향:" + dir);
    }
}

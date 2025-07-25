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
        //TODO 플레이어 기능의 물감발사를 가져오자
        Debug.Log("타겟에게 발사");
    }
}

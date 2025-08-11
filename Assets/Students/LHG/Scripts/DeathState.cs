using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : AIBaseState
{
    private float _deathTime = 10f;
    private bool _respawnScheduled = false;
    private Coroutine routine;

    public DeathState(AIController controller) : base(controller) { }

    public override void OnEnter()
    {
        _controller.col.enabled = false;
        _controller.humanModel.SetActive(false);

        if (!_respawnScheduled)
        { 
            routine = _controller.StartCoroutine(RespawnDelay());
            _respawnScheduled = true; 
        }
    }


    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(_deathTime);
        _controller.Respawn();
    }


    public override void OnExit()
    {
        _controller.IsDeadState = false;
        if (routine != null) 
        { 
            _controller.StopCoroutine(routine); 
            routine = null; 
        }
        _respawnScheduled = false;
    }

    public override void OnUpdate()
    { 
    }
}

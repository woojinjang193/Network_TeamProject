using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireModule
{
    private AIController _controller;
    private float nextFireTime;

    private PhotonView weaponView;

    public FireModule(AIController controller, PhotonView weaponView)
    {
        _controller = controller;
        this.weaponView = weaponView;
    }
    //public void FireAt(Transform target)
    //{
    //    if (Time.time >= nextFireTime)
    //    {
    //        nextFireTime = Time.time + _controller.fireInterval;
    //        Vector3 dir = (target.position - _controller.transform.position).normalized;
    //        Debug.DrawRay(_controller.transform.position, dir * 5f, Color.red, 0.2f); //TODO 플레이어 기능의 물감발사를 가져오자 + 발사간격

    //        //타겟으로 회전
    //        _controller.transform.forward = dir;

    //        //발사on
    //        if(!_controller.inkGun.mainEmission.enabled)

    //        //inkparticlecollision이것만 껐다켰다하면된다고함
    //    }
    //}

    public void TryFireAt(Transform target) // TODO : 타겟 사용안함
    {
        FireStart();
    }

    private void FireStart()
    {
        if (!_controller.isFiring)
        {
            _controller.isFiring = true;
            weaponView.RPC("FireParticle", RpcTarget.All, _controller.MyTeam, true);
        }
    }

    public void StopFire()
    {
        if (_controller.isFiring)
        {
            _controller.isFiring = false;
            weaponView.RPC("FireParticle", RpcTarget.All, _controller.MyTeam, false);
        }
    }
}

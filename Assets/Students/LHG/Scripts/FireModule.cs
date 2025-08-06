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

    public void TryFireAt(Transform target) // TODO : 타겟 사용안함
    {
        FireStart();
    }

    private void FireStart()
    {
        if (!_controller.IsFiring)
        {
            _controller.IsFiring = true;
            weaponView.RPC("FireParticle", RpcTarget.All, _controller.MyTeam, true);
        }
    }

    public void StopFire()
    {
        if (_controller.IsFiring)
        {
            _controller.IsFiring = false;
            weaponView.RPC("FireParticle", RpcTarget.All, _controller.MyTeam, false);
        }
    }
}

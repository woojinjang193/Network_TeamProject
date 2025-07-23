using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private GameObject inkProjectilePrefab;
    [SerializeField] Transform muzzlePoint;
    [SerializeField] private float projectileSpeed = 30f;

    private Rigidbody projectileRigid;

    [PunRPC]
    public void FireInk(Team team, PhotonMessageInfo info)
    {
        GameObject projectile = Instantiate(inkProjectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        //���� ������Ʈ Ǯ������ ���� 

        //������
        InkProjectile newProjectile = projectile.GetComponent<InkProjectile>();
        newProjectile.SetTeam(team);

        projectileRigid = newProjectile.GetComponent<Rigidbody>();


        //��������
        float lag = Mathf.Abs((float)PhotonNetwork.Time - (float)info.SentServerTime);

        projectile.transform.position += projectile.transform.forward * projectileSpeed * lag;
        projectileRigid.AddForce(transform.forward * projectileSpeed, ForceMode.Impulse);
    }

}

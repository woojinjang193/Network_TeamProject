using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHP;

    private float curHP;

    private void Start()
    {
        curHP = maxHP;
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        curHP -= amount;
        Debug.Log($"현재 체력{curHP}");

        if (curHP <= 0)
        {
            Debug.Log("플레이어 죽음");
        }
        
    }

}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    private float curHP;
    private bool isDead = false;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        curHP = maxHP;
    }

    [PunRPC]
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        curHP -= amount;
        Debug.Log($"현재 체력{curHP}");

        if (curHP <= 0)
        {
            curHP = 0;
            isDead = true;
            Debug.Log("플레이어 죽음");

            playerController.Die();
        }
    }

    public void Respawn()
    {
        isDead = false;
        curHP = maxHP;
        Debug.Log("체력 초기화");
    }

}

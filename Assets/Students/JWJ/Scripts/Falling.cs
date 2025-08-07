using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falling : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Debug.Log("플레이어 낙사");
            player.FallingDeath();
        }

        else if (other.TryGetComponent<AIController>(out AIController bot))
        {
            Debug.Log("봇 낙사");
            bot.TakeDamage(444f);
        }
    }
}

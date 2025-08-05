using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falling : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<BaseController>(out BaseController baseController))
        {
            Debug.Log("플레이어 충돌"); 
            baseController.TakeDamage(333);

        }
    }
}

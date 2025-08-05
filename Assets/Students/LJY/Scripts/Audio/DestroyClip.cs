using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyClip : MonoBehaviour
{
    private AudioSource audioSource;
    private PlayerController player;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void LateUpdate()
    {
        if (!player.IsFiring)
        {
            audioSource.Stop();
            Destroy(gameObject);
        }
    }

    public void SetController(PlayerController controller)
    {
        player = controller;
    }
}

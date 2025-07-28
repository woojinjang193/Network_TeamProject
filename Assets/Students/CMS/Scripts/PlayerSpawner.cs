using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        // Photon 방 안에 있는 상태에서만 스폰
        if (PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            PhotonNetwork.Instantiate("PlayerPrefab", spawnPos, Quaternion.identity);
        }
    }
}

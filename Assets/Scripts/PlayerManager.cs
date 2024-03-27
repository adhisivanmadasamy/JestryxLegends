using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{

    PhotonView PV;
    GameObject controller;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    
    void Start()
    {
        if(PV.IsMine)
        {
            CreateController();
        }    
    }

    void CreateController()
    {
        Debug.Log("Player Controller Created");
        Transform spawnpoint = SpawnManager.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs","PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID});
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        Debug.Log("Player dead");

        CreateController();
        Debug.Log("Player Respawned");
    }
    
}

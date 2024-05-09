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
        Transform spawnpoint = SpawnManager.instance.GetSpawnPoint();
        string CharName = (string)PhotonNetwork.LocalPlayer.CustomProperties["SelectedAgent"];
        if(CharName == "Connor")
        {
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ConnorPlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
        }
        else if(CharName == "Chloe")
        {
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ChloePlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
        }
        else if(CharName == "Ryan")
        {
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "RyanPlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
        }
        
        Debug.Log(CharName + "Player Controller Created");
    }

    public void Die()
    {        
        MenuManager.instance.OpenMenu("DeadPanel");
        controller.GetComponent<PlayerController>().isDead = true;        

        Debug.Log("Player dead");
       
    }
    
}

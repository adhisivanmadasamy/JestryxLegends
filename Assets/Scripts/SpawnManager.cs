using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public GameObject AttackSpawn, DefenceSpawn;

    Spawnpoint[] spawnpoints;

    string PlayModeAttack = "Attack";
    string PlayModeDefense = "Defense";

    private void Awake()
    {
        instance = this;

        int TeamIndex = Random.Range(0, 1);
        string[] currentAttack = { "TeamA", "TeamB" };
        

        
        if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == currentAttack[TeamIndex])
        {
            spawnpoints = AttackSpawn.GetComponentsInChildren<Spawnpoint>();
            Debug.Log("No of Spawnpoints: "+spawnpoints.Length);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PlayMode", PlayModeAttack } });
        }
        else
        {
            spawnpoints = DefenceSpawn.GetComponentsInChildren<Spawnpoint>();
            Debug.Log("No of Spawnpoints: " + spawnpoints.Length);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PlayMode", PlayModeDefense } });
        }
    }

        

    public Transform GetSpawnPoint()
    {
        string Agent = (string)PhotonNetwork.LocalPlayer.CustomProperties["SelectedAgent"];
        int index = 0;
        if(Agent == "Connnor")
        {
            index = 0;
        }
        else if(Agent == "Chloe")
        {
            index= 1;
        }
        else if(Agent == "Ryan")
        {
            index = 2;
        }

        Debug.Log("Index:"+index+"Name: "+Agent);

        return spawnpoints[index].transform;
    }
}

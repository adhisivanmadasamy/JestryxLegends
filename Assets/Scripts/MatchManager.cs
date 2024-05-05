using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchManager : MonoBehaviour
{

    public int OwnScore, EnemyScore;
    public float currentTime, MaxBuyTime, MaxMatchTime, MaxDefuseTime;

    public bool isBuyPhase, isMatchPhase = false, isDefusePhase = false;

    public TextMeshProUGUI TimeText;

    public bool isBuyWindowOpen;

    public PlayerController localPlayerController;

    public GameObject SpikeSpawnLocation;

    public GameObject Spike;


    // Start is called before the first frame update
    void Start()
    {        
        BuyTimeStart();
        SpawnSpike();
       
    }

    // Update is called once per frame
    void Update()
    {
        if(isBuyPhase)
        {
            BuyTimerRun();
            ToggleBuy();
        }
        else if(isMatchPhase)
        {
            MatchTimerRun();
        }
        else if (isDefusePhase)
        {
            
        }
    }


    public void SpawnSpike()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Spike = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "SpikeObj"), SpikeSpawnLocation.transform.position, SpikeSpawnLocation.transform.rotation);
            Debug.Log("Spike spawned");
        }
    }

    public void BuyTimeStart()
    {
        currentTime = MaxBuyTime;
        isBuyPhase = true;
    }

    public void MatchTimeStart()
    {
        currentTime = MaxMatchTime;
        isMatchPhase = true;
    }

    public void DefuseTimeStart()
    {
        currentTime = MaxDefuseTime;
        isDefusePhase = true;
    }

    public void BuyTimerRun()
    {
        currentTime -= Time.deltaTime;
        TimeText.text = FormatTime(currentTime);
        if (currentTime <=0f)
        {
            isBuyPhase = false;
            MenuManager.instance.OpenMenu("HUD");
            MatchTimeStart();
        }
    }

    public void MatchTimerRun()
    {
        currentTime -= Time.deltaTime;
        TimeText.text = FormatTime(currentTime);
        if (currentTime <= 0f)
        {
            isMatchPhase = false;
            MatchOver();
        }
    }

    public void MatchOver()
    {

    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ToggleBuy()
    {
        if(isBuyPhase)
        {
            if(Input.GetKeyDown(KeyCode.B))
            {
                if(MenuManager.instance.MenuBool("BuyPanel"))
                {
                    MenuManager.instance.OpenMenu("HUD");
                }
                else
                {
                    MenuManager.instance.OpenMenu("BuyPanel");
                }
                
            }
        }
    }

    public void BuyPistol()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Check if the player exists and owns the PhotonView
        if (player != null && player.GetPhotonView().IsMine)
        {
            PlayerController localPlayerController = player.GetComponent<PlayerController>();

            if (localPlayerController != null)
            {
                // Update the items array locally
                localPlayerController.items[0].itemGameObject.SetActive(false);
                localPlayerController.items = new Item[] { localPlayerController.Pistol, localPlayerController.Knife };
                localPlayerController.items[0].itemGameObject.SetActive(true);
                Debug.Log("Bought Pistol");

                // Call RPC to synchronize changes across the network
                localPlayerController.photonView.RPC("SyncItems", RpcTarget.All, localPlayerController.items);
            }
        }
    }


    public void BuyRifle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (GetComponent<Photon.Pun.PhotonView>().IsMine)
        {
            // This GameObject represents the local player
            localPlayerController = player.GetComponent<PlayerController>();
            Debug.Log("PV is mine");

            if (localPlayerController != null)
            {
                localPlayerController.items[0].itemGameObject.SetActive(false);
                localPlayerController.items = new Item[] { localPlayerController.Rifle, localPlayerController.Knife };
                localPlayerController.items[0].itemGameObject.SetActive(true);
                Debug.Log("Bought Rifle");

            }
        }
    }

    public void TestButton()
    {
        Debug.Log("Test Button");
    }

    


    // Define the items array and other variables here

    


}

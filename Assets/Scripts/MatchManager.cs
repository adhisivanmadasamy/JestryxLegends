using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchManager : MonoBehaviour
{

    public int OwnScore, EnemyScore;
    public TextMeshProUGUI OwnScoreUI, EnemyScoreUI;

    public float currentTime, MaxBuyTime, MaxMatchTime, MaxDefuseTime;

    public bool isBuyPhase =true, isMatchPhase = false, isDefusePhase = false;

    public TextMeshProUGUI TimeText;

    public GameObject IconBuyPhase, IconSpike;

    public bool isBuyWindowOpen;

    public PlayerController localPlayerController;

    public GameObject SpikeSpawnLocation;

    public GameObject Spike;

    public GameObject PlantArea1, PlantArea2;

    public bool isMatchOver = false;
    public float MaxEndTime = 5f;

    public string WonTeam;
    private int currentCameraIndex = 0;
    public RenderTexture renderTexture;

    public ScoreManager scoreManager;

    public bool RoundOver = false;

    public PhotonView PV;
    public Player localPlayer;

    public string localPlayMode;

    List<Camera> teamCameras = new List<Camera>();

    // Start is called before the first frame update
    void Start()
    {        
        BuyTimeStart();
        SpawnSpike();
        PV = GetComponent<PhotonView>();   
        
        scoreManager = FindObjectOfType<ScoreManager>();

        localPlayer = PhotonNetwork.LocalPlayer;        
        GameObject localObj = localPlayer.TagObject as GameObject;

        localPlayMode = (string)localPlayer.CustomProperties["PlayMode"];

        

    }

    // Update is called once per frame
    void Update()
    {
        if(isBuyPhase)
        {
            IconBuyPhase.SetActive(true);
            BuyTimerRun();
            ToggleBuy();
        }
        else if(isMatchPhase)
        {
            IconBuyPhase.SetActive(false);
            MatchTimerRun();
        }
        else if (isDefusePhase)
        {
            Destroy(PlantArea1);
            Destroy(PlantArea2);
            IconBuyPhase.SetActive(false);
            IconSpike.SetActive(true);
            DefuseTimerRun();
        }
        else if(isMatchOver)
        {
            IconSpike.SetActive(false);
            EndTimerRun();

        }
    }

    public void UIScoreUpdate()
    {        

        if (localPlayMode != null)
        {
            if (localPlayMode == "Attack")
            {
                OwnScore = scoreManager.attackScore;
                EnemyScore = scoreManager.defenseScore;
                OwnScoreUI.text = OwnScore.ToString();
                EnemyScoreUI.text = EnemyScore.ToString();
            }
            else if (localPlayMode == "Defense")
            {
                OwnScore = scoreManager.defenseScore;
                EnemyScore = scoreManager.attackScore;
                OwnScoreUI.text = OwnScore.ToString();
                EnemyScoreUI.text = EnemyScore.ToString();
            }
        }
        else
        {
            Debug.Log("Local playMode null, MatchManager UIScoreUpdate");
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

    public void EndTimerStart(string team)
    {
        WonTeam = team;
        Debug.Log("Won team: " + WonTeam);
        currentTime = MaxEndTime;
        isMatchPhase = false;
        isDefusePhase = false;
        isMatchOver = true;

        if(WonTeam == "Attack")
        {
            AttackWon();
            
            //Level Reload or server data send
            Debug.Log("Going to Next level: Attack Won");
            if (PhotonNetwork.IsMasterClient)
            {
                //Score update
                scoreManager.attackScore += 1;
                scoreManager.UpdateScoreToRoom();
            }
            else
            {
                Debug.Log("Not master client, waiting...");
            }            
        }
        else if (WonTeam == "Defense")
        {
            DefenseWon();
            
            //Level Reload or server data send
            Debug.Log("Going to Next level: Defense Won");

            if (PhotonNetwork.IsMasterClient)
            {
                scoreManager.defenseScore += 1;
                scoreManager.UpdateScoreToRoom();
            }
            else
            {
                Debug.Log("Not master client, waiting...");
            }

            
        }       
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

            EndTimerStart("Defense");
        }
    }

    public void DefuseTimerRun()
    {
        currentTime -= Time.deltaTime;
        TimeText.text = FormatTime(currentTime);
        if (currentTime <= 0f)
        {
            isDefusePhase = false;           
            EndTimerStart("Attack");
        }
    }

    public void EndTimerRun()
    {
        
        currentTime -= Time.deltaTime;
        TimeText.text = FormatTime(currentTime);
        if( currentTime <= 0f)
        {
            if(RoundOver == false)
            {
                string team = WonTeam;
                scoreManager.CheckMatchOver();
                RoundOver = true;
                
            }
            
        }
    }

    public void AttackWon()
    {
        Debug.Log("Attackers Won");
        PV.RPC("RPC_Won", RpcTarget.All, "Attack");
        
    }

    public void DefenseWon()
    {
        Debug.Log("Defense Won");
        PV.RPC("RPC_Won", RpcTarget.All, "Defense");
    }


    [PunRPC]
    public void RPC_Won(string Mode)
    {
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["PlayMode"] == Mode)
        {
            MenuManager.instance.OpenMenu("Victory");
        }
        else
        {
            MenuManager.instance.OpenMenu("Defeat");
        }
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
    public void SpectateInit()
    {
        
        string localPlayMode = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayMode"];
        string localPlayerName = (string)PhotonNetwork.LocalPlayer.NickName;
        // Find all GameObjects with the "PlayerCamera" tag
        PlayerCam[] playerCameraHolders = FindObjectsOfType<PlayerCam>();
        Debug.Log("No of Camera Holders: " + playerCameraHolders.Length);

        foreach (PlayerCam playerCameraHolder in playerCameraHolders)
        {
            PhotonView playerPV = playerCameraHolder.gameObject.GetComponentInParent<PhotonView>();
            if((string)playerPV.Owner.CustomProperties["PlayMode"] == localPlayMode)
            {
                if(playerPV.Owner.NickName != localPlayerName)
                {
                    Camera camera = playerCameraHolder.GetComponentInChildren<Camera>();
                    teamCameras.Add(camera);
                }
                
            }

            
        }
        
        Debug.Log("Cameras count: " + teamCameras.Count);
        Debug.Log(teamCameras[0] + " is " + renderTexture);
        teamCameras[0].targetTexture = renderTexture;
    }
       

    public void CycleNextCamera()
    {
        if (teamCameras.Count == 0)
        {
            Debug.LogWarning("No cameras found for spectating.");
            return;
        }

        currentCameraIndex = (currentCameraIndex + 1) % teamCameras.Count;
        UpdateRenderTexture();
    }
    public void UpdateRenderTexture()
    {
        Camera currentCamera = teamCameras[currentCameraIndex];
        currentCamera.GetComponent<Camera>().targetTexture = renderTexture;

        // Reset previous camera's target texture
        foreach (Camera camera in teamCameras)
        {
            if (camera != currentCamera)
            {
                camera.targetTexture = null;
            }
        }
    }

    public void RemoveCamera(Camera cameraToRemove)
    {
        if (teamCameras.Contains(cameraToRemove))
        {
            teamCameras.Remove(cameraToRemove);
            if (teamCameras.Count > 0 && currentCameraIndex >= teamCameras.Count)
            {
                currentCameraIndex = 0;
            }
            UpdateRenderTexture();
        }
    }










}

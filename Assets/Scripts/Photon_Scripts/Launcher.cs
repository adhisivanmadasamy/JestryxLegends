using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    public TMP_InputField RoomNameInputField;
    public TextMeshProUGUI RoomNameText;
    public TextMeshProUGUI ErrorText;

    public Transform RoomListContent;
    public GameObject RoomListItemPrefab;



    public Transform PlayerListContent;
    public GameObject PlayerListItemPrefab;

    public Transform TeamListContent;

    public GameObject StartGameButton;

    public TextMeshProUGUI TeamNameText, PlayerNameText;

    public float TimeDuration = 30f;
    public bool isTimerOn = false;
    public float CurrentTime;
    public TextMeshProUGUI TimeText;

    public bool ComingBack = false;


    private const string AttackScoreKey = "AttackScore";
    private const string DefenseScoreKey = "DefenseScore";

    private int attackScore = 0;
    private int defenseScore = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {        
        Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();   
    }

    public void Update()
    {
        TimerRun();
    }

    public void BackToRoom()
    {
        MenuManager.instance.OpenMenu("inroompanel");

    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to MasterServer");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("main");
        Debug.Log("Joined MasterServer lobby");
        PhotonNetwork.NickName = "Player" + Random.Range(0, 100).ToString("000");
    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(RoomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(RoomNameInputField.text);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("inroompanel");
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Created and joined room: " + RoomNameText.text);

        Player[] players = PhotonNetwork.PlayerList;

        foreach(Transform child in PlayerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(PlayerListItemPrefab, PlayerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);

        }

        StartGameButton.SetActive(PhotonNetwork.IsMasterClient);

    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        StartGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("errorpanel");
        ErrorText.text = message;
    }


    public void StartGame()
    {        
        SeparatePlayersIntoTeams();        
    }


    public void StartTimer()
    {
        isTimerOn = true;
        CurrentTime = TimeDuration;
    }

    public void TimerRun()
    {
        if (isTimerOn)
        {
            CurrentTime -= Time.deltaTime;
            TimeText.text = Mathf.Round(CurrentTime).ToString();
            if (CurrentTime <= 0f)
            {
                isTimerOn= false;
                OpenMainLevel();
            }
        }
    }
    void SeparatePlayersIntoTeams()
    {
        Player[] players = PhotonNetwork.PlayerList;
        int playerCount = players.Length;    
       

        // Assign remaining players to balance teams
        for (int i = 0; i < playerCount; i++)
        {
            string team = (i % 2 == 0) ? "TeamA" : "TeamB";
            players[i].SetCustomProperties(new Hashtable { { "Team", team } });
            Debug.Log("Player: " + players[i].NickName + "/Team is " + team);
        }

        OpenAgentSelectionPanel();
    }
    public void OpenAgentSelectionPanel()
    {
        // Ensure that only the host calls the RPC to avoid unnecessary network traffic
        
            photonView.RPC("RPC_OpenAgentSelectionPanel", RpcTarget.AllViaServer);
        
    }

    [PunRPC]
    void RPC_OpenAgentSelectionPanel()
    {
        MenuManager.instance.OpenMenu("agentpanel");
        StartTimer();

        // Clear existing content in the agent selection panel
        foreach (Transform child in TeamListContent)
        {
            Destroy(child.gameObject);
        }

       
        string localPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        string PlayerName = (string)PhotonNetwork.LocalPlayer.NickName;


        TeamNameText.text = localPlayerTeam;
        if(TeamNameText.text == "TeamA")
        {
            TeamNameText.color = Color.red;
        }
        else
        {
            TeamNameText.color = Color.blue;
        }       
        
        PlayerNameText.text = PlayerName;
        

        // Display only players from the local player's team
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player player in players)
        {
            string playerTeam = (string)player.CustomProperties["Team"];            

            if (playerTeam == localPlayerTeam)
            {
                GameObject playerUI = Instantiate(PlayerListItemPrefab, TeamListContent);
                PlayerListItem playerListItem = playerUI.GetComponent<PlayerListItem>();
                if (playerListItem != null)
                {
                    playerListItem.SetUp(player);
                    Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is in team:" + localPlayerTeam);
                }
            }
        }
    }


    public void OpenMainLevel()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            SetLevel1();
            PhotonNetwork.LoadLevel(1);
        }
       
    }

    public void SetLevel1()
    {
        ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
        roomProps[AttackScoreKey] = 0;
        roomProps[DefenseScoreKey] = 0;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");

    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("main");
        Debug.Log("left the room: " + RoomNameText.text);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in RoomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                continue;
            }
            Instantiate(RoomListItemPrefab, RoomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, PlayerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }


}

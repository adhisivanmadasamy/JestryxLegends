using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using System.Net.Sockets;
using static Photon.Pun.UtilityScripts.PunTeams;

public class AgentSelectionManager : MonoBehaviourPunCallbacks
{
    //public GameObject playerListItemPrefab; // Prefab for displaying player UI
    public Transform teamListContent; // Content area where player UI elements will be displayed
    public GameObject lockButton; // Button to lock in agent selection

    public GameObject ConnorButton, ChloeButton, RyanButton;
    //public GameObject[] agentButtons; // Buttons for selecting agents (Connor, Chloe, Ryan)

    private bool agentsLocked = false; // Flag to track if agent selection is locked

    private Dictionary<int, string> selectedAgents = new Dictionary<int, string>(); // Dictionary to store selected agents for each player

    

    
    public void SelectAgent(int agentIndex)
    {
        if (!agentsLocked)
        {
            // Get the selected agent name based on the index
            string selectedAgent = "";
            switch (agentIndex)
            {
                case 0:
                    selectedAgent = "Connor";
                    break;
                case 1:
                    selectedAgent = "Chloe";
                    break;
                case 2:
                    selectedAgent = "Ryan";
                    break;
            }

            // Update selected agent for the local player
            selectedAgents[PhotonNetwork.LocalPlayer.ActorNumber] = selectedAgent;

            // Update UI for the local player
            UpdatePlayerUI(PhotonNetwork.LocalPlayer.ActorNumber, selectedAgent);

            // Sync selected agent across all players
            photonView.RPC("SyncSelectedAgent", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, selectedAgent);

            //Enabling the lock button after selecting a character
            lockButton.SetActive(true);
        }
    }

    public void LockAgents()
    {
        agentsLocked = true;
        lockButton.SetActive(false); // Hide the lock button after locking

        // Update selected agent in the player's custom properties
        string selectedAgent = selectedAgents[PhotonNetwork.LocalPlayer.ActorNumber];
        Hashtable hash = new Hashtable();
        hash.Add("SelectedAgent", selectedAgent);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
       
        //Hiding the Char buttons
        ConnorButton.SetActive(false);
        RyanButton.SetActive(false);
        ChloeButton.SetActive(false);

        //Local Player's Team
        string currentPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];

        // Locally done the locking, so syncing to other players
        photonView.RPC("SyncLockStatus", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, agentsLocked, selectedAgent, currentPlayerTeam);
                
    }

    
    void UpdatePlayerUI(int actorNumber, string selectedAgent)
    {
        
            //GameObject[] playerUIs = GameObject.FindGameObjectsWithTag("PlayerUI"); // Find all instantiated player UI prefabs

            foreach (Transform child in teamListContent)
            {
                PlayerListItem playerListItem = child.GetComponent<PlayerListItem>();
                // Find the TextMeshProUGUI component displaying the player's nickname
                TextMeshProUGUI playerNameText = playerListItem.GetComponentInChildren<TextMeshProUGUI>();

                // Check if the player's nickname matches the provided nickname
                if (playerNameText != null && playerNameText.text == PhotonNetwork.PlayerList[actorNumber - 1].NickName)
                {
                    // Find the Agent TextMeshProUGUI component
                    TextMeshProUGUI agentText = playerListItem.transform.Find("Agent").GetComponent<TextMeshProUGUI>();

                    // Update the agent's name
                    if (agentText != null)
                    {
                        agentText.text = selectedAgent;
                    }

                    // Exit the loop once we've found and updated the correct player UI
                    break;
                }
            }
        
    }

    

    

    [PunRPC]
    void SyncSelectedAgent(int actorNumber, string selectedAgent)
    {
        selectedAgents[actorNumber] = selectedAgent;
        UpdatePlayerUI(actorNumber, selectedAgent);
    }

    

    [PunRPC]
    void SyncLockStatus(int actorNumber, bool locked, string selectedAgent, string team)
    {

        string currentPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        if (team == currentPlayerTeam)
        {

            if (selectedAgent == "Connor")
            {
                ConnorButton.SetActive(false);
            }
            else if (selectedAgent == "Chloe")
            {
                ChloeButton.SetActive(false);
            }
            else if (selectedAgent == "Ryan")
            {
                RyanButton.SetActive(false);
            }

        }
    }
}

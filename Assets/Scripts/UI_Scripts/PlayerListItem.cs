using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI text;
    Player player;
    public TextMeshProUGUI AgentSelected;

    public void SetUp(Player _player)
    {
        player = _player;
        text.text = _player.NickName;
    }

    public void UpdateAgent(string agentName)
    {
        AgentSelected.gameObject.SetActive(true);
        AgentSelected.text = agentName;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(otherPlayer == player)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}

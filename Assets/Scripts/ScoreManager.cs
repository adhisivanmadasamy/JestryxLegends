using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public MatchManager matchManager;

    public int TeamCount, EnemyCount;

    private const string AttackScoreKey = "AttackScore";
    private const string DefenseScoreKey = "DefenseScore";

    public int attackScore = 0;
    public int defenseScore = 0;

    public PhotonView PV;


    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        matchManager = FindObjectOfType<MatchManager>();

        InitializeScoresFromRoomProperties();
        

    }

    // Update is called once per frame
    void Update()
    {

    }



    private void InitializeScoresFromRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(AttackScoreKey, out object attackScoreObj))
        {
            attackScore = (int)attackScoreObj;
            Debug.Log("Attack Score: " + attackScore);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(DefenseScoreKey, out object defenseScoreObj))
        {
            defenseScore = (int)defenseScoreObj;
            Debug.Log("Defense Score: " + defenseScore);
        }

        UpdateScoreUI();

    }

    public void UpdateScoreUI()
    {
        matchManager.UIScoreUpdate();

    }
    public void UpdateScoreToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
            roomProps[AttackScoreKey] = attackScore;
            roomProps[DefenseScoreKey] = defenseScore;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

            Debug.Log("Updated in room custom properties");

        }

    }

    public void CheckMatchOver()
    {
        if(attackScore >= 1 ||  defenseScore >= 1)
        {
            Debug.Log("Game over");

            
            
            
        }
        else
        {
            PhotonNetwork.LoadLevel(1);
        }
        

        
           
            
        
        
    }

    [PunRPC]
    public void RPC_LoadLevelForAll()
    {
        PhotonNetwork.LoadLevel(1);
    }

    
}

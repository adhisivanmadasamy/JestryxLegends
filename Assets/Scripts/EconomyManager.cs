using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class EconomyManager : MonoBehaviour
{

    

    public int playerCoins = 0;

    public int initialCoins = 100;
    public int winningCoin = 1600;
    public int losingCoin = 1200;
    public int killCoin = 20;

    public TextMeshProUGUI playerCoinsText;

    public MatchManager matchManager;
    // Start is called before the first frame update
    void Start()
    {
        playerCoins = initialCoins;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PlayerCoins", playerCoins } });
    }

    // Update is called once per frame
    void Update()
    {
        if (matchManager.isBuyPhase)
        {
            playerCoinsText.text = Mathf.Round(playerCoins).ToString();
        }
        
    }

    public void Purchase(int price)
    {
        if ((int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerCoins"] >= price)
        {
            playerCoins -= price;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PlayerCoins", playerCoins } });
            if(price == 80)
            {
                matchManager.BuyPistol();
            }
            else if(price == 140)
            {
                matchManager.BuyRifle();
            }
            Debug.Log("Purchased for: "+playerCoins);
        }
        else
        {
            Debug.Log("Not enough money");
        }
        
    }

    public void Reward(int price)
    {
        
            playerCoins += price;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PlayerCoins", playerCoins } });
            Debug.Log("Rewarded: " + playerCoins);
        
    }

    
}

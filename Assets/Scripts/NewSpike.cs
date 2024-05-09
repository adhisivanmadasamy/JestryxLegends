using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSpike : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        PhotonView otherPhotonView = other.GetComponent<PhotonView>();
        if (otherPhotonView != null && otherPhotonView.Owner != null && otherPhotonView.Owner.CustomProperties != null)
        {
            string playMode = (string)otherPhotonView.Owner.CustomProperties["PlayMode"];
            if (playMode == "Defense")
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController != null && !playerController.CanDefuse)
                {
                    playerController.CanDefuse = true;
                    playerController.isPlanted = true;
                    Debug.Log("Can defuse: " + playerController.CanDefuse.ToString());
                }
            }
        }
    }


    public void OnTriggerExit(Collider other)
    {
        PhotonView otherPhotonView = other.GetComponent<PhotonView>();
        if (otherPhotonView != null && otherPhotonView.Owner != null && otherPhotonView.Owner.CustomProperties != null)
        {
            string playMode = (string)otherPhotonView.Owner.CustomProperties["PlayMode"];
            if (playMode == "Defense")
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController != null && !playerController.CanDefuse)
                {
                    playerController.CanDefuse = false;
                    Debug.Log("Can defuse: " + playerController.CanDefuse.ToString());
                }
            }
        }
    }
}

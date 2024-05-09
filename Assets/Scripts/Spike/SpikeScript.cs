using Photon.Pun;
using UnityEngine;

public class SpikeScript : MonoBehaviourPun
{
    public bool isPicked = false;
    public PlayerController playerController;



    private void OnTriggerEnter(Collider other)
    {
        if (!isPicked && other.CompareTag("Player"))
        {
            PhotonView photonView = other.GetComponent<PhotonView>();
            if ((string)photonView.Owner.CustomProperties["PlayMode"] == "Attack")
            {
                if (photonView.GetComponent<PlayerController>().isDead == false)
                {
                    isPicked = true;
                    playerController = other.GetComponent<PlayerController>();
                    playerController.hasSpike = true;
                    playerController.PickUpSpike(this.gameObject);
                }                                      

            }                
        }
    }   


}

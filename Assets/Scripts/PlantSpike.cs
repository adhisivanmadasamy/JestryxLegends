using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpike : MonoBehaviour
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
        if((string)other.GetComponent<PhotonView>().Owner.CustomProperties["PlayMode"] == "Attack")
        {
            PlayerController EnterController = other.GetComponent<PlayerController>();
            if (EnterController.hasSpike == true)
            {
                EnterController.CanPlant = true;
                Debug.Log("Can Plant");
            }
        }
        
    }

    

    public void OnTriggerExit(Collider other)
    {
        if ((string)other.GetComponent<PhotonView>().Owner.CustomProperties["PlayMode"] == "Attack")
        {
            PlayerController ExitController = other.GetComponent<PlayerController>();
            Debug.Log(ExitController.hasSpike.ToString());
            if (ExitController.hasSpike == true)
            {
                ExitController.CanPlant = false;
                Debug.Log("Can not Plant");
            }
        }
    }


    
}

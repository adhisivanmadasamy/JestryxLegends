using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;
    [SerializeField] GameObject cameraHolder;

    public Item[] items;

    public Item Pistol, Rifle, Knife;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    public Vector3 moveAmount;

    Rigidbody rb;
    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;

    float scaledJumpforce;

    public bool hasSpike;

    public GameObject SpikeCanvas;

    public bool CanPlant = false;
    public bool isPlanting = false;
    public float plantTimer = 0f, PlantMaxTime = 4f;
    public bool isPlanted = false;

    public bool CanDefuse =false;
    public bool isDefusing =false;
    public float defuseTimer = 0f, DefuseMaxTime = 4f;
    public bool isDefused = false;

    public bool isDead = false;

    //
    public AudioManager playeraudio;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
       

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        playeraudio = GetComponentInChildren<AudioManager>();
        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }


    }
    private void Update()
    {
        if (isDead == false)
        {
            if (!PV.IsMine)
            return;

        
            Look();
            Move();
                       

            //Weapon Equip with scroll
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                if (itemIndex >= items.Length - 1)
                {
                    EquipItem(0);
                }
                else
                {
                    EquipItem(itemIndex + 1);
                }
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                if (itemIndex <= 0)
                {
                    EquipItem(items.Length - 1);
                }
                else
                {
                    EquipItem(itemIndex - 1);
                }

            }

            if (Input.GetMouseButtonDown(0))
            {
                items[itemIndex].Use();
            }

            // - - - -

            //Planting Update
            if (isPlanted == false)
            {
                if (hasSpike == true && CanPlant == true && isPlanting == false && Input.GetKeyDown(KeyCode.P))
                {
                    StartPlanting();
                    Debug.Log("Planting Started");
                }

                if (isPlanting)
                {
                    UpdatePlantTimer();
                }

                if (isPlanting)
                {
                    if (!Input.GetKey(KeyCode.P))
                    {
                        isPlanting = false;
                        Debug.Log("Plant interupted");
                    }
                }

            }


            // - - - -

            //Defuse Update
            if (isPlanted == true && isDefused == false)
            {
                if (CanDefuse == true && isDefusing == false && Input.GetKeyDown(KeyCode.P))
                {
                    StartDefusing();
                    Debug.Log("Defusing started");
                }

                if (isDefusing)
                {
                    UpdateDiffuseTimer();
                    if (!Input.GetKey(KeyCode.P))
                    {
                        isDefusing = false;
                        Debug.Log("Defuse Interupted");
                    }
                }

            }


        }

        // Only look after dead
        if(isDead == true)
        {
            Look();
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }   


    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -70f, 70f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;

    }

    void Move()
    {
        playeraudio.runSound();
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && grounded)
        {
            playeraudio.jumpSound();
            scaledJumpforce = jumpForce * Time.fixedDeltaTime;
            rb.AddForce(transform.up * scaledJumpforce, ForceMode.Impulse);
        }
    }

    public void EquipItem(int _index)
    {
        if (previousItemIndex == _index)
            return;

        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("ItemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }


    //Sync Guns when changed across players
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        
        if (!PV.IsMine && targetPlayer == PhotonNetwork.LocalPlayer)
        {
            //EquipItem((int)changedProps["ItemIndex"]);
        }
    }

    
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    private void FixedUpdate()
    {
        Jump();

        if (!PV.IsMine)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);

        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    //PickUp Spike
    public void PickUpSpike(GameObject gameObject)
    {
        hasSpike = true;
        gameObject.transform.parent = transform;
        gameObject.transform.GetComponentInChildren<MeshRenderer>().enabled = false;
        gameObject.transform.GetComponent<BoxCollider>().enabled = false;
        int ownerViewID = PV.ViewID;
        Debug.Log("SPike picked - Own");

        SpikeCanvas.SetActive(true);
        PV.RPC("RPC_Sync_Spike", RpcTarget.OthersBuffered, ownerViewID);
    }      
    


   //Planting Spike

    public void StartPlanting()
    {
        playeraudio.spikeplantedSound();
        isPlanting = true;
        plantTimer = 0f;
    }
    
    //Defuse Start
    public void StartDefusing()
    {
        isDefusing = true;
        defuseTimer = 0f;
    }

    //Update plant
    public void UpdatePlantTimer()
    {
        plantTimer += Time.deltaTime;
        if ((plantTimer >= PlantMaxTime))
        {
            SpikePlanted();
            isPlanting=false;
            isPlanted = true;
            plantTimer = 0;
        }
        
    }


    //Update defuse
    public void UpdateDiffuseTimer()
    {
        defuseTimer += Time.deltaTime;
        if( defuseTimer > DefuseMaxTime )
        {
            SpikeDefused(); 
                      
        }
    }

    //Spike defused
    public void SpikeDefused()
    {
        isDefused = true;
        Debug.Log("Spike Defused");

        MatchManager matchManager = FindObjectOfType<MatchManager>();
        matchManager.EndTimerStart("Defense");

        //RPC Call
        PV.RPC("RPC_Defused", RpcTarget.OthersBuffered);
    }

    //Spike planted - Local
    public void SpikePlanted()
    {
        
        Transform oldSpike = transform.Find("SpikeObj(Clone)");
        if (oldSpike != null)
        {
            oldSpike.parent = null;
            Transform NewSpikeSpawn = oldSpike.transform;
            Destroy(GameObject.FindGameObjectWithTag("Spike"));
            Debug.Log("Destroyed Old spike");

            GameObject NewSpike = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NewSpike"), NewSpikeSpawn.position, NewSpikeSpawn.rotation);
            Debug.Log("Spawned New Spike");

            GameObject[] PlantAreas = GameObject.FindGameObjectsWithTag("PlantArea");
            Destroy(PlantAreas[0]);
            Destroy(PlantAreas[1]);            

            isPlanted = true;
            MatchManager matchManager = FindObjectOfType<MatchManager>();
            matchManager.isMatchPhase = false;
            matchManager.DefuseTimeStart();

            PV.RPC("RPC_Test", RpcTarget.OthersBuffered);

            int PVID = PV.ViewID;
            PV.RPC("RPC_Planted", RpcTarget.OthersBuffered, PVID);

            SpikeCanvas.SetActive(false);

            isPlanted = true;
        }
        else
        {
            Debug.LogError("SpikeObj(Clone) GameObject not found!");
        }

        Debug.Log("Spike planted");
    }

    //Spike drop - Local
    public void DropSpike()
    {
        
        Transform oldSpike = transform.Find("SpikeObj(Clone)");
        if(oldSpike != null)
        {
            oldSpike.parent = null;
            oldSpike.GetComponentInChildren<MeshRenderer>().enabled = true;
            oldSpike.GetComponent<BoxCollider>().enabled = true;
            oldSpike.GetComponent<SpikeScript>().isPicked = false;
            Debug.Log("Spike droppped - own");
        }
        else
        {
            Debug.Log("Error - Couldn't find spike");
        }

        int PViewID = PV.ViewID;
        PV.RPC("RPC_Drop", RpcTarget.OthersBuffered, PViewID);

        hasSpike = false;
    }


    //Take Damage
    public void TakeDamage(float damage)
    {

        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }


    //Die
    void Die()
    {
        if(PV.GetComponent<PlayerController>().hasSpike == true)
        {
            DropSpike();
        }
        
        playerManager.Die();

    }


    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        Debug.Log("Took Damage: " + damage);

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {            
            Die();
        }
    }


    [PunRPC]
    public void RPC_Sync_Spike(int ownerViewID)
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("Spike");
        PhotonView ownerView = PhotonView.Find(ownerViewID);
        if (ownerView != null)
        {
            Transform ownerTransform = ownerView.transform;
            gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
            gameObject.transform.GetComponent<BoxCollider>().enabled = false;
            gameObject.transform.parent = ownerTransform;
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["PlayMode"] == "Attack")
            {
                ownerView.GetComponent<PlayerController>().SpikeCanvas.SetActive(true);
                ownerView.GetComponent<PlayerController>().hasSpike = true;
            }            
        }
        Debug.Log("Spike picked up - sync");
        return;
    }

    [PunRPC]
    public void RPC_Test()
    {
        Debug.Log("Test RPC call working");
    }
    
    [PunRPC]
    public void RPC_Planted(int PVID)
    {
        Debug.Log("Sync Start ///");

        PlayerController plantedPlayerController = PhotonView.Find(PVID).GetComponent<PlayerController>();
        GameObject plantedGO = plantedPlayerController.gameObject;

        plantedPlayerController.SpikeCanvas.SetActive(false);

        GameObject spike = GameObject.FindGameObjectWithTag("Spike");
        Destroy(spike); 
        Debug.Log("Old spike destroyed");

        PV.GetComponent<PlayerController>().isPlanted = true;
        Debug.Log("isPlanted: " + PV.GetComponent<PlayerController>().isPlanted.ToString());


        MatchManager matchManager2 = FindObjectOfType<MatchManager>();
        matchManager2.isMatchPhase = false;
        matchManager2.DefuseTimeStart();

        Debug.Log("Sync End ///");
    }

    [PunRPC]
    public void RPC_Drop(int viewID)
    {
        PlayerController plantedPlayerController = PhotonView.Find(viewID).GetComponent<PlayerController>();
        GameObject plantedGO = plantedPlayerController.gameObject;
        

        GameObject spike = GameObject.FindGameObjectWithTag("Spike");
        if(spike != null)
        {
            spike.transform.parent = null;
            spike.GetComponentInChildren<MeshRenderer>().enabled = true;
            spike.GetComponent<BoxCollider>().enabled = true;
            spike.GetComponent<SpikeScript>().isPicked = false;
        }
        else
        {
            Debug.Log("Couldn't find spike");
        }
        

        plantedPlayerController.hasSpike = false;
        plantedPlayerController.SpikeCanvas.SetActive(false);
        
        Debug.Log("Spike droppped - sync");

    }

    [PunRPC]
    public void RPC_Defused()
    {
        PV.GetComponent<PlayerController>().isDefused = true;
        Debug.Log("Spike Defused");

        MatchManager matchManager = FindObjectOfType<MatchManager>();
        matchManager.EndTimerStart("Defense");

        Debug.Log("End synced");
    }

    [PunRPC]
    public void SyncItems(Item[] updatedItems)
    {
        // Update the items array with the synchronized values
        items = updatedItems;
    }

    
    

    

}

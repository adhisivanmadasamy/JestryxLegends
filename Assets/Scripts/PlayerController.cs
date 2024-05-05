using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
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
        if (!PV.IsMine)
            return;

        Look();
        Move();



        //for(int i = 0; i < items.Length; i++)
        // {
        //     if(Input.GetKeyDown((i + 1).ToString()))
        //     {
        //        EquipItem(i); 
        //        break;
        //    }
        //}

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
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);

    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && grounded)
        {
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


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["ItemIndex"]);

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
        gameObject.transform.parent = transform;
        gameObject.transform.GetComponentInChildren<MeshRenderer>().enabled = false;
        int ownerViewID = photonView.ViewID;
        photonView.RPC("RPC_Sync_Spike", RpcTarget.OthersBuffered, ownerViewID);
    }

    public void DropSpike()
    {
        // Set custom property to indicate that the player has dropped the spike
        Hashtable hash = new Hashtable();
        hash.Add("HasSpike", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        // Other logic for dropping the spike
    }


   
    

    public void TakeDamage(float damage)
    {

        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
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
    private void RPC_Sync_Spike(int ownerViewID)
    {
        GameObject gameObject = GameObject.FindGameObjectWithTag("Spike");
        PhotonView ownerView = PhotonView.Find(ownerViewID);
        if (ownerView != null)
        {
            Transform ownerTransform = ownerView.transform;
            gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
            gameObject.transform.parent = ownerTransform;
        }
    }

    [PunRPC]
    public void SyncItems(Item[] updatedItems)
    {
        // Update the items array with the synchronized values
        items = updatedItems;
    }

    [PunRPC]
    public void DestroySpikeOthers()
    {

    }
    

    void Die()
    {
        playerManager.Die();

    }

}

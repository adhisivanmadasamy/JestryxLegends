using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    public PlayerController controller;

    public void Start()
    {
        controller = GetComponentInParent<PlayerController>();
    }
    public override void Use()
    {
        Shoot();
    }

    public void Shoot()
    {
        controller.playeraudio.pistolSound();
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if(Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            PhotonView hitPhotonView = hitInfo.collider.gameObject.GetComponent<PhotonView>();
            if ((hitPhotonView != null))
            {
                string shooterTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];                
                string hitPlayerTeam = (string)hitPhotonView.Owner?.CustomProperties["Team"];   
                if(hitPlayerTeam != shooterTeam)
                {
                    Debug.Log("Hit on: " + hitInfo.collider.gameObject.name);
                    hitInfo.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                }            
            }            
        }
    }
}

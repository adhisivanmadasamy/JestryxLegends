using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatKnife : Knife
{
    [SerializeField] Camera cam;
    [SerializeField] float maxDistance = 2f;
    public Animator animator;

    public PlayerController playerController;

    public void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponentInParent<PlayerController>();
    }
    public override void Use()
    {
        Kill();
    }


    public void Kill()
    {
        animator.SetTrigger("Kill");
        playerController.playeraudio.KnifeSound();
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance))
        {
            PhotonView hitPhotonView = hitInfo.collider.gameObject.GetComponent<PhotonView>();
            if ((hitPhotonView != null))
            {
                string shooterTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                string hitPlayerTeam = (string)hitPhotonView.Owner?.CustomProperties["Team"];
                if (hitPlayerTeam != shooterTeam)
                {
                    Debug.Log("Hit on: " + hitInfo.collider.gameObject.name);
                    hitInfo.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((KnifeInfo)itemInfo).damage);
                }
            }

        }   
        
         //animator.SetBool("KnifeKill", false);

    }

}
